/*
 * Phase 15 YARP API Gateway.
 * This project routes browser/API traffic to microservices and deliberately
 * keeps business logic out of the gateway.
 */
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderFlow.Gateway.Auth;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.Configure<BasicAuthenticationOptions>(builder.Configuration.GetSection("GatewayAuth"));
builder.Services.AddAuthentication("Basic")
    .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("Basic", null);
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Portals", policy =>
    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService("OrderFlow.Gateway"))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddConsoleExporter());

var app = builder.Build();
app.UseRateLimiter();
app.UseCors("Portals");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Text("""
OrderFlow Gateway is running.

Health:
  GET /health

Proxied API routes:
  /api/customers
  /api/products
  /api/inventory
  /api/orders
  /api/checkout
  /api/payments
  /api/notifications

Portals:
  Customer Portal: http://localhost:5173
  Admin Portal:    http://localhost:4200
""", "text/plain"));
app.MapHealthChecks("/health");
app.MapReverseProxy().RequireAuthorization();

app.Run();
