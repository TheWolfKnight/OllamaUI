
using System.Text.Json.Serialization;

namespace OllamaUi.Caller.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<OllamaChatMessageRole>))]
public enum OllamaChatMessageRole
{
  System = 0,
  User,
  Assistant,
  Tool
}
