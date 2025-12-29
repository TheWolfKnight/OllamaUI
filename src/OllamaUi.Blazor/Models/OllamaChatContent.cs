
namespace OllamaUi.Blazor.Models;

public class OllamaChatContent
{
  public required string Prompt { get; init; }
  public required string Response { get; set; }
  public string? ThinkingText {get; set; }
}
