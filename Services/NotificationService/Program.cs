using Microsoft.EntityFrameworkCore;
using OrderFlow.NotificationService.Data;
using OrderFlow.NotificationService.Messaging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy("Portals", policy =>
    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddHostedService<RabbitMqNotificationConsumer>();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Portals");
app.MapHealthChecks("/health");
using (var scope = app.Services.CreateScope()) await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.EnsureCreatedAsync();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.ExecuteSqlRawAsync("""
IF COL_LENGTH('Notifications', 'SourceEventId') IS NULL
    ALTER TABLE [Notifications] ADD [SourceEventId] nvarchar(100) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notifications_SourceEventId' AND object_id = OBJECT_ID('Notifications'))
    CREATE UNIQUE INDEX [IX_Notifications_SourceEventId] ON [Notifications] ([SourceEventId]) WHERE [SourceEventId] IS NOT NULL;
""");
}
app.MapControllers();

app.Run();
