/* RabbitMQ consumer with duplicate handling, retries, and a dead-letter queue. */
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.NotificationService.Data;
using OrderFlow.NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderFlow.NotificationService.Messaging;

public sealed class RabbitMqNotificationConsumer(IServiceProvider services, IConfiguration configuration, ILogger<RabbitMqNotificationConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Run(() => Consume(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private void Consume(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = configuration["RabbitMQ:HostName"] ?? "localhost", DispatchConsumersAsync = true };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var exchange = configuration["RabbitMQ:Exchange"] ?? "orderflow.events";
            var queue = configuration["RabbitMQ:Queue"] ?? "notification-service.order-events";
            var dlq = configuration["RabbitMQ:DeadLetterQueue"] ?? "notification-service.order-events.dlq";

            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
            channel.QueueDeclare(dlq, durable: true, exclusive: false, autoDelete: false);
            channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object> { ["x-dead-letter-exchange"] = "", ["x-dead-letter-routing-key"] = dlq });
            channel.QueueBind(queue, exchange, "order.*");
            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, args) =>
            {
                var retries = args.BasicProperties.Headers is not null && args.BasicProperties.Headers.TryGetValue("x-retry-count", out var value) ? Convert.ToInt32(value) : 0;
                try
                {
                    await HandleMessageAsync(args, stoppingToken);
                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex) when (retries < 3)
                {
                    logger.LogWarning(ex, "Notification consumer failed. Retrying message {MessageId}.", args.BasicProperties.MessageId);
                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.MessageId = args.BasicProperties.MessageId;
                    props.Headers = new Dictionary<string, object> { ["x-retry-count"] = retries + 1 };
                    channel.BasicPublish(exchange, args.RoutingKey, props, args.Body);
                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Notification consumer moved message {MessageId} to DLQ.", args.BasicProperties.MessageId);
                    channel.BasicNack(args.DeliveryTag, false, requeue: false);
                }
            };
            channel.BasicConsume(queue, autoAck: false, consumer);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ is unavailable. NotificationService HTTP API still runs.");
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken ct)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());
        var integrationEvent = JsonSerializer.Deserialize<IntegrationEvent>(json);
        if (integrationEvent is null) return;

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var eventId = integrationEvent.EventId.ToString();
        if (await db.Notifications.AnyAsync(x => x.SourceEventId == eventId, ct)) return;

        db.Notifications.Add(new Notification
        {
            OrderId = integrationEvent.OrderId,
            Recipient = "async-notification@oms.local",
            Message = $"{integrationEvent.EventType} received from RabbitMQ.",
            Status = "Queued",
            SourceEventId = eventId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}

public sealed record IntegrationEvent(Guid EventId, string EventType, int OrderId, DateTime OccurredAtUtc, JsonElement Payload);
