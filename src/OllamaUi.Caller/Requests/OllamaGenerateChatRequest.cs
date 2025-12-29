using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Requests;

public class OllamaGenerateChatRequest
{
  [JsonPropertyName("model")]
  public required string Model { get; init; }
  [JsonPropertyName("messages")]
  public required IEnumerable<OllamaChatRequestMessage> Messages { get; init; }

  public bool think { get; init; } = true;
  public string? KeepAlive { get; init; }
}
