using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OrderFlow.AiService.Dtos;
using OrderFlow.AiService.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for portals
builder.Services.AddCors(options => options.AddPolicy("Portals", policy =>
    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:4200", "http://127.0.0.1:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Configure Gateway HTTP Client (passes basic auth for microservice endpoints)
var gatewayBaseUrl = builder.Configuration["Gateway:BaseUrl"] ?? "http://localhost:6200/api/";
builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri(gatewayBaseUrl.TrimEnd('/') + "/");
    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("orderflow-admin:orderflow-dev-password"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
});

// Configure Tools
builder.Services.AddSingleton<OrderFlowTools>();

// Configure Ollama and IChatClient with Function Calling
var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
var ollamaModel = builder.Configuration["Ollama:Model"] ?? "llama3.2";

builder.Services.AddSingleton<IChatClient>(_ =>
{
    var ollama = new OllamaApiClient(new Uri(ollamaEndpoint), ollamaModel);
    return ((IChatClient)ollama).AsBuilder().UseFunctionInvocation().Build();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Portals");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "AiService", model = ollamaModel }));

app.MapPost("/api/ai/chat", async (
    [FromBody] ChatRequest request,
    IChatClient chatClient,
    OrderFlowTools tools,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
        return Results.BadRequest(new { error = "Message cannot be empty." });

    var systemInstructions = new StringBuilder();
    systemInstructions.AppendLine("You are the OrderFlow AI Assistant for the OrderFlow customer portal.");
    systemInstructions.AppendLine("You help customers with their order status, past order history, catalog products, stock availability, and store policies (returns, refunds, shipping, delivery, cancellations).");
    systemInstructions.AppendLine("When answering policy questions (e.g., return window, shipping fees, cancellation terms), ALWAYS call SearchStorePoliciesAsync to get accurate, up-to-date policy documents.");
    systemInstructions.AppendLine("Use the provided tools to fetch real-time information from OrderFlow services.");
    systemInstructions.AppendLine("When reporting order status, always mention the Order ID, status, item list, and total formatted as currency.");
    systemInstructions.AppendLine("Keep answers clear, polite, and concise.");

    if (request.CustomerId.HasValue)
        systemInstructions.AppendLine($"The currently logged-in customer has Customer ID: {request.CustomerId.Value} and Email: {request.CustomerEmail}. If the user asks about 'my orders' or 'my status', use this customer ID.");

    var messages = new List<ChatMessage>
    {
        new(ChatRole.System, systemInstructions.ToString())
    };

    if (request.History is not null)
    {
        foreach (var past in request.History)
        {
            var role = past.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) 
                ? ChatRole.Assistant 
                : ChatRole.User;
            messages.Add(new(role, past.Content));
        }
    }

    messages.Add(new(ChatRole.User, request.Message));

    var chatOptions = new ChatOptions
    {
        Tools =
        [
            AIFunctionFactory.Create(tools.GetCustomerOrdersAsync),
            AIFunctionFactory.Create(tools.GetLatestOrderAsync),
            AIFunctionFactory.Create(tools.GetOrderDetailsAsync),
            AIFunctionFactory.Create(tools.CheckInventoryAsync),
            AIFunctionFactory.Create(tools.ListCatalogProductsAsync),
            AIFunctionFactory.Create(tools.SearchStorePoliciesAsync)
        ]
    };

    try
    {
        var response = await chatClient.GetResponseAsync(messages, chatOptions, ct);
        return Results.Ok(new AiChatResponse(response.Text ?? "I could not generate a response."));
    }
    catch (Exception ex)
    {
        return Results.Ok(new AiChatResponse($"[Assistant Notice] I encountered an error communicating with the local model: {ex.Message}"));
    }
});

app.Run();
