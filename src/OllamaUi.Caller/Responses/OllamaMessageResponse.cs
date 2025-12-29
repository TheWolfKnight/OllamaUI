using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Responses;

public class OllamaMessageResponse
{
  [JsonPropertyName("model")]
  public required string Model { get; init; }
  [JsonPropertyName("created_at")]
  public required DateTime CreatedAt { get; init; }

  [JsonPropertyName("response")]
  public required string Response { get; init; }
  [JsonPropertyName("thinking")]
  public string? Thinking { get; init; }

  [JsonPropertyName("done")]
  public required bool Done { get; init; }
  [JsonPropertyName("done_reason")]
  [MemberNotNullWhen(true, nameof(Done))]
  public string ? DoneReason { get; init; }

  [JsonPropertyName("total_duration")]
  public long? TotalDuration { get; init; }
  [JsonPropertyName("load_duration")]
  public long? LoadDuration { get; init; }
  [JsonPropertyName("prompt_eval_duration")]
  public long? PromptEvalDuration { get; init; }
  [JsonPropertyName("eval_duration")]
  public long? EvalDuration { get; init; }

  [JsonPropertyName("prompt_eval_count")]
  public long? PromptEvalCount { get; init; }
  [JsonPropertyName("eval_count")]
  public long? EvalCount { get; init; }

  [JsonPropertyName("logprops")]
  public OllamaMessageResponseLogProp[]? LogProps { get; init; }
}
