/* RabbitMQ publisher for Phase 11 asynchronous communication. */
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderFlow.OrderService.Messaging;

public interface IEventPublisher
{
    void Publish(string routingKey, IntegrationEvent integrationEvent);
}

public sealed class RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger) : IEventPublisher
{
    public void Publish(string routingKey, IntegrationEvent integrationEvent)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = configuration["RabbitMQ:HostName"] ?? "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var exchange = configuration["RabbitMQ:Exchange"] ?? "orderflow.events";
            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = integrationEvent.EventId.ToString();
            properties.Type = integrationEvent.EventType;

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));
            channel.BasicPublish(exchange, routingKey, properties, body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RabbitMQ publish failed. The HTTP checkout remains complete, but async notification is skipped.");
        }
    }
}
