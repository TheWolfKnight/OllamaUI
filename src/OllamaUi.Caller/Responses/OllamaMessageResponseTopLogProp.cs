using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Responses;

public class OllamaMessageResponseTopLogProp
{
  [JsonPropertyName("token")]
  public required string Token { get; init; }
  [JsonPropertyName("logprop")]
  public required float LogProb { get; init; }
  [JsonPropertyName("bytes")]
  public required byte[] Bytes { get; init; }
}
