using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using OrderFlow.OrderService.Clients;
using OrderFlow.OrderService.Data;
using OrderFlow.OrderService.Messaging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("Portals", policy =>
    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 180, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService("OrderFlow.OrderService"))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddConsoleExporter());
builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductService"]!);
    client.Timeout = TimeSpan.FromSeconds(3);
}).AddStandardResilienceHandler();
builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:InventoryService"]!);
    client.Timeout = TimeSpan.FromSeconds(3);
}).AddStandardResilienceHandler();
builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentService"]!);
    client.Timeout = TimeSpan.FromSeconds(3);
}).AddStandardResilienceHandler();
builder.Services.AddHttpClient<INotificationClient, NotificationClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:NotificationService"]!);
    client.Timeout = TimeSpan.FromSeconds(3);
}).AddStandardResilienceHandler();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Portals");
app.UseRateLimiter();
app.MapHealthChecks("/health");
using (var scope = app.Services.CreateScope()) await scope.ServiceProvider.GetRequiredService<OrderDbContext>().Database.EnsureCreatedAsync();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Orders', 'Total') IS NULL
    ALTER TABLE [Orders] ADD [Total] decimal(18,2) NOT NULL CONSTRAINT [DF_Orders_Total] DEFAULT 0;
IF COL_LENGTH('OrderItems', 'ProductName') IS NULL
    ALTER TABLE [OrderItems] ADD [ProductName] nvarchar(200) NOT NULL CONSTRAINT [DF_OrderItems_ProductName] DEFAULT '';
IF OBJECT_ID(N'[CheckoutSagas]', N'U') IS NULL
    CREATE TABLE [CheckoutSagas] (
        [Id] int NOT NULL IDENTITY,
        [IdempotencyKey] nvarchar(100) NOT NULL,
        [OrderId] int NULL,
        [CurrentStep] nvarchar(100) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [FailureReason] nvarchar(max) NULL,
        [StartedAtUtc] datetime2 NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_CheckoutSagas] PRIMARY KEY ([Id])
    );
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CheckoutSagas_IdempotencyKey' AND object_id = OBJECT_ID('CheckoutSagas'))
    CREATE UNIQUE INDEX [IX_CheckoutSagas_IdempotencyKey] ON [CheckoutSagas] ([IdempotencyKey]);
""");
}
app.MapControllers();

app.Run();
