
namespace OllamaUi.Blazor.Models;

public class OllamaChatContent
{
  public required string Prompt { get; init; }
  public required string Response { get; set; }
  public required string ThinkingText { get; set; }

  public bool ShowThinking { get; set; } = true;
  public bool ActiveThinking { get; set; } = true;

  public bool Done { get; set; }
}
