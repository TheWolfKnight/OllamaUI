using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Responses;

public class OllamaMessageResponseLogProp
{
  [JsonPropertyName("token")]
  public required string Token { get; init; }
  [JsonPropertyName("logprop")]
  public required float LogProb { get; init; }
  [JsonPropertyName("bytes")]
  public required byte[] Bytes { get; init; }
  [JsonPropertyName("top_logprops")]
  public required OllamaMessageResponseTopLogProp[] TopLogProps { get; init; }
}
