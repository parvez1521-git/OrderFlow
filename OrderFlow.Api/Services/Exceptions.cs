/* Domain-facing exception used when a request violates a simple business rule.
 * Controllers convert it into an appropriate HTTP error response. */
namespace OrderFlow.Api.Services;

public sealed class BusinessRuleException(string message) : Exception(message);
