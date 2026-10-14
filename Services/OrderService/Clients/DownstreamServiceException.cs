/* Exception used when a synchronous downstream microservice call fails. */
namespace OrderFlow.OrderService.Clients;

public sealed class DownstreamServiceException(string serviceName, string message) : Exception(message)
{
    public string ServiceName { get; } = serviceName;
}
