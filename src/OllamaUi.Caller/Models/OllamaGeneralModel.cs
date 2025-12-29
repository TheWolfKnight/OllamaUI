using System;
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Models;

public class OllamaGeneralModel: OllamaModel
{
  [JsonPropertyName("name")]
  public required string Name { get; init; }
  [JsonPropertyName("modified_at")]
  public required DateTime ModifiedAt { get; init; }
}
