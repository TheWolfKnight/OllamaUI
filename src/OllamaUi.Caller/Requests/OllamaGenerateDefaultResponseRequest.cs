using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Requests;

public class OllamaGenerateDefaultResponseRequest: OllamaGenerateResponseRequest
{
  [JsonPropertyName("prompt")]
  public required string Prompt { get; init; }
}
