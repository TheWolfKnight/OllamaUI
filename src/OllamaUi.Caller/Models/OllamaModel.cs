using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Models;

public abstract class OllamaModel
{
  [JsonPropertyName("size")]
  public required long Size { get; init; }
  [JsonPropertyName("digest")]
  public required string Digest { get; init; }
  [JsonPropertyName("details")]
  public required OllamaModelDetails Details { get; init; }
}
