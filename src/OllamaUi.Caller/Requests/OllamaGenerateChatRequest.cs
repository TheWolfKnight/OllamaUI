using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Requests;

public class OllamaGenerateChatRequest
{
  [JsonPropertyName("model")]
  public required string Model { get; init; }
  [JsonPropertyName("messages")]
  public required IEnumerable<OllamaChatRequestMessage> Messages { get; init; }

  [JsonPropertyName("think")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
  public bool Think { get; init; } = true;
  [JsonPropertyName("keep_alive")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? KeepAlive { get; init; }

  [JsonPropertyName("options")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public OllamaChatOptions? Options { get; init; }
}

public class OllamaChatOptions
{
  ///<summary>
  ///  The amount of tokens the model is allowed to keep in context at once
  ///</summary>
  [JsonPropertyName("num_ctx")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public int? ContextSize { get; init; }
}
