using System.Text.Json.Serialization;
using OllamaUi.Caller.Enums;

namespace OllamaUi.Caller.Responses;

public class OllamaChatResponseMessage
{
  [JsonPropertyName("role")]
  public required OllamaChatMessageRole Role { get; init; }
  [JsonPropertyName("content")]
  public required string Content { get; init; }
  [JsonPropertyName("thinking")]
  public string? Thinking { get; init; }
}
