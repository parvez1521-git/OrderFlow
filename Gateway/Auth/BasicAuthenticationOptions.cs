/*
 * Phase 15 gateway authentication options.
 * These settings keep the sample Basic-auth credentials configurable instead of
 * hard-coding them inside the authentication handler.
 */
using Microsoft.AspNetCore.Authentication;

namespace OrderFlow.Gateway.Auth;

public sealed class BasicAuthenticationOptions : AuthenticationSchemeOptions
{
    public string UserName { get; set; } = "orderflow-admin";

    public string Password { get; set; } = "orderflow-dev-password";
}
