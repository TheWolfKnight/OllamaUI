using System;
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Models;

public class OllamaRunningModel: OllamaModel
{
  [JsonPropertyName("model")]
  public required string Model { get; init; }
  [JsonPropertyName("expires_at")]
  public required DateTime ExpiresAt { get; init; }
  [JsonPropertyName("size_vram")]
  public required long SizeVram { get; init; }
  [JsonPropertyName("context_length")]
  public required int ContextLength { get; init; }
}
