using System.Text.Json;
using OllamaUi.Blazor.Models;
using Microsoft.AspNetCore.Components;

namespace OllamaUi.Blazor.Components.Pages;

public partial class Home: ComponentBase
{
  [Inject]
  public required PageState PageState { get; init; }
  [Inject]
  public required ILogger<Home> Logger { get; init; }

  private List<Manifest> _knownChats = [];
  private Manifest _render = new ManifestSpecial
  {
    ChatId = Guid.Empty,
    DisplayName = "chat",
    CreationDate = DateTime.MinValue,
    OllamaModelName = "",
  };
  private SetupNewChat? _newChat;

  protected override async Task OnInitializedAsync()
  {
    string saveLocation = Path.Combine(PageState.ChatSaveLocation);
    if (!Directory.Exists(saveLocation))
      return;

    DirectoryInfo info = new DirectoryInfo(saveLocation);
    foreach (DirectoryInfo path in info.GetDirectories())
    {
      string manifestPath = Path.Combine(path.FullName, "manifest.json");
      using FileStream manifestContent = File.OpenRead(manifestPath);

      JsonSerializerOptions options = new JsonSerializerOptions
      {
        AllowTrailingCommas = true,
        WriteIndented = true,
      };
      Manifest? manifest = await JsonSerializer.DeserializeAsync<Manifest>(manifestContent, options: options);

      if (manifest is null)
      {
        Logger.LogWarning(
          "Manifest for chat id {ChatId} could not be read",
          path.Name
        );
        continue;
      }

      _knownChats.Add(manifest);
    }

    await Task.CompletedTask;
  }

  private async Task OnNewChatSentAsync(SetupNewChat setup)
  {
    Console.WriteLine(setup.ChatId.ToString());

    Manifest manifest = await WriteChatManifestAsync(setup);

    _knownChats.Add(manifest);
    _newChat = setup;
    _render = manifest;

    StateHasChanged();

    return;
  }

  private async Task<Manifest> WriteChatManifestAsync(SetupNewChat setup)
  {
    string chatPath = Path.Combine(PageState.ChatSaveLocation, setup.ChatId.ToString("d"));
    if (!Directory.Exists(chatPath))
    {
      Directory.CreateDirectory(chatPath);
      string chatFiles = Path.Combine(chatPath, "messages");
      Directory.CreateDirectory(chatFiles);
    }

    const int displayNameMaxLenght = 25;

    Manifest manifest = new Manifest_V1
    {
      ChatId = setup.ChatId,
      CreationDate = DateTime.UtcNow,
      DisplayName = setup.Prompt[..(Math.Min(setup.Prompt.Length, displayNameMaxLenght))],
      OllamaModelName = setup.ModelName
    };

    JsonSerializerOptions options = new JsonSerializerOptions
    {
      AllowTrailingCommas = true,
      WriteIndented = true
    };
    string manifestContent = JsonSerializer.Serialize(manifest, options: options);

    string manifestPath = Path.Combine(chatPath, "manifest.json");
    using FileStream fileStream = File.OpenWrite(manifestPath);
    using StreamWriter writer = new StreamWriter(fileStream);

    await writer.WriteLineAsync(manifestContent);

    return manifest;
  }

  private async Task OnElementClickedAsync(Manifest manifest)
  {
    _render = manifest;
    await Task.CompletedTask;
  }
}
