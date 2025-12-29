using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Requests;

[JsonDerivedType(typeof(OllamaGenerateDefaultResponseRequest), "default")]
public abstract class OllamaGenerateResponseRequest
{
  [JsonPropertyName("model")]
  public required string Model { get; init; }
}
