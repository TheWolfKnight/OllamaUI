using OllamaUi.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace OllamaUi.Blazor.Components.Views;

public partial class SidebarComponent: ComponentBase
{
  [Parameter]
  public Manifest? SelectedElement { get; set; } = null!;
  [Parameter, EditorRequired]
  public required EventCallback<Manifest> SelectedElementChanged { get; init; }

  [Parameter, EditorRequired]
  public required List<Manifest> SavedChats { get; init; }

  private ManifestSpecial _messageManifest = new()
  {
    ChatId = Guid.Empty,
    CreationDate = DateTime.MinValue,
    DisplayName = "message",
    OllamaModelName = ""
  };
  private ManifestSpecial _newChatManifest = new()
  {
    ChatId = Guid.Empty,
    CreationDate = DateTime.MinValue,
    DisplayName = "chat",
    OllamaModelName = ""
  };

  protected override Task OnInitializedAsync()
  {
    SelectedElement = _newChatManifest;
    return Task.CompletedTask;
  }

  private async Task OnElementClickedAsync(Manifest manifest)
  {
    SelectedElement = manifest;
    if (SelectedElementChanged is EventCallback<Manifest> callback)
      await callback.InvokeAsync(manifest);
  }

  private string GetClass(string ident)
  {
    string element = SelectedElement switch
    {
      ManifestSpecial special => special.DisplayName,
      Manifest_V1 v1 => v1.ChatId.ToString("d"),
      _ => throw new InvalidDataException("Invalid manifest type")
    };

    return ident == element
      ? "selected"
      : "";
  }
}
