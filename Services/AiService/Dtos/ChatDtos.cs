namespace OrderFlow.AiService.Dtos;

public sealed record ChatRequest(
    string Message,
    int? CustomerId = null,
    string? CustomerEmail = null,
    List<ChatMessageDto>? History = null
);

public sealed record ChatMessageDto(string Role, string Content);

public sealed record AiChatResponse(string Reply);
