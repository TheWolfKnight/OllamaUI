using System.Text.Json.Serialization;
using OllamaUi.Caller.Enums;

namespace OllamaUi.Caller.Requests;

public class OllamaChatRequestMessage
{
  [JsonPropertyName("role")]
  public required OllamaChatMessageRole Role { get; init; }
  [JsonPropertyName("content")]
  public required string Content { get; init; }
}
