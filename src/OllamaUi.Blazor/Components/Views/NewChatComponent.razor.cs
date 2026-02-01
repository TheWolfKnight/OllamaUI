using OllamaUi.Blazor.Models;
using OllamaUi.Caller.Models;
using OllamaUi.Caller.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OllamaUi.Blazor.Components.Views;

public partial class NewChatComponent: ComponentBase
{
  [Inject]
  public required IOllamaModelCaller ModelCaller { get; init; }
  [Inject]
  public required IJSRuntime JSRuntime { get; init; }
  [Inject]
  public required PageState PageState { get; init; }
  [Inject]
  public required ILogger<NewChatComponent> Logger { get; init; }

  ///<summary>
  /// Returns the prompt from the user, when starting a new chat.
  /// The new Chat Id is sent before the prompt
  ///</summary>
  [Parameter, EditorRequired]
  public required EventCallback<SetupNewChat> SetupNewChat { get; init; }

  private IEnumerable<OllamaGeneralModel> _models = [];
  private OllamaGeneralModel? _selectedModel = null;
  private string _userInput = string.Empty;

  protected override async Task OnInitializedAsync()
  {
    var models = await ModelCaller.GetOllamaModelsAsync(default);
    if (models.IsFailed)
    {
      Logger.LogWarning(
        "Failed to load available models, cannot start chat"
      );
      return;
    }

    _models = models.Value;
    _selectedModel = models.Value.FirstOrDefault();

    StateHasChanged();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender)
      return;

    await JSRuntime.InvokeVoidAsync("bind_ctrl_enter", new object[0]);
  }

  private async Task OnPromptSentAsync()
  {
    if (string.IsNullOrWhiteSpace(_userInput) || _selectedModel is null)
      return;

    Guid chatId;

    bool found = false;
    do
    {
      chatId = Guid.NewGuid();
      string chatSaveLocation = Path.Combine(PageState.ChatSaveLocation, chatId.ToString("d"));

      if (!Directory.Exists(chatSaveLocation))
        found = true;
    }
    while (!found);

    SetupNewChat content = new SetupNewChat
    {
      Prompt = _userInput,
      ChatId = chatId,
      ModelName = _selectedModel.Name
    };

    if (SetupNewChat is EventCallback<SetupNewChat> setupCallback)
      await setupCallback.InvokeAsync(content);
  }
}
