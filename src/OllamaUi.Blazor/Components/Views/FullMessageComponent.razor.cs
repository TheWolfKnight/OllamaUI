using OllamaUi.Blazor.Models;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace OllamaUi.Blazor.Components.Views;

public partial class FullMessageComponent: ComponentBase
{
  [Parameter, EditorRequired]
  public required OllamaChatContent Message { get; init; }

  private string Prompt => Markdown.ToHtml(Message.Prompt);
  private string Thinking => Markdown.ToHtml(Message.ThinkingText);
  private string Response => Markdown.ToHtml(Message.Response);

  private bool _shouldRender = false;

  protected override bool ShouldRender()
  {
    if (_shouldRender)
    {
      _shouldRender = false;
      return true;
    }

    return false;
  }

  private string GetThinkingClass()
  {
    return string.Empty;
  }

  private void ChangeShowThinking()
  {
    _shouldRender = true;
    Message.ShowThinking = !Message.ShowThinking;
  }
}
