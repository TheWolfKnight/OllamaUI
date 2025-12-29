using System;
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Responses;

public class OllamaChatResponse
{
  [JsonPropertyName("model")]
  public required string Model { get; init; }
  [JsonPropertyName("created_at")]
  public required DateTime CreatedAt { get; init; }
  [JsonPropertyName("message")]
  public required OllamaChatResponseMessage Message { get; init; }
  [JsonPropertyName("done")]
  public required bool Done { get; init; }
}
