/*
 * Phase 17 .NET Aspire AppHost.
 * Orchestrates local OrderFlow services plus Redis and RabbitMQ for learning.
 */
var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var rabbitMq = builder.AddRabbitMQ("rabbitmq");

builder.AddProject<Projects.OrderFlow_Api>("identityapi");

var customerService = builder.AddProject<Projects.OrderFlow_CustomerService>("customerservice");
var productService = builder.AddProject<Projects.OrderFlow_ProductService>("productservice").WithReference(redis);
var inventoryService = builder.AddProject<Projects.OrderFlow_InventoryService>("inventoryservice");
var paymentService = builder.AddProject<Projects.OrderFlow_PaymentService>("paymentservice");
var notificationService = builder.AddProject<Projects.OrderFlow_NotificationService>("notificationservice").WithReference(rabbitMq);

var orderService = builder.AddProject<Projects.OrderFlow_OrderService>("orderservice")
    .WithReference(rabbitMq)
    .WithEnvironment("ServiceUrls__ProductService", productService.GetEndpoint("http"))
    .WithEnvironment("ServiceUrls__InventoryService", inventoryService.GetEndpoint("http"))
    .WithEnvironment("ServiceUrls__PaymentService", paymentService.GetEndpoint("http"))
    .WithEnvironment("ServiceUrls__NotificationService", notificationService.GetEndpoint("http"));

builder.AddProject<Projects.OrderFlow_Gateway>("gateway")
    .WithReference(customerService)
    .WithReference(productService)
    .WithReference(inventoryService)
    .WithReference(orderService)
    .WithReference(paymentService)
    .WithReference(notificationService);

builder.Build().Run();
