using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Models;

public class OllamaModelDetails
{
  [JsonPropertyName("parent_model")]
  public string? ParentModel { get; init; }
  [JsonPropertyName("format")]
  public required string Format { get; init; }
  [JsonPropertyName("family")]
  public required string Family { get; init; }
  [JsonPropertyName("families")]
  public required string[] Families { get; init; }
  [JsonPropertyName("parameter_size")]
  public required string ParameterSize { get; init; }
  [JsonPropertyName("quantization_level")]
  public required string QuantizationLevel { get; init; }
}
