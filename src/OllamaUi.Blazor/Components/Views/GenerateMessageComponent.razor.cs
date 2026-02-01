using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Models;
using OllamaUi.Caller.Requests;
using OllamaUi.Caller.Responses;
using Microsoft.AspNetCore.Components;

namespace OllamaUi.Blazor.Components.Views;

public partial class GenerateMessageComponent: ComponentBase
{
  [Inject]
  public required IOllamaGenerationCaller OllamaGenerationCaller { get; init; }
  [Inject]
  public required IOllamaModelCaller OllamaModelCaller { get; init; }
  [Inject]
  public required ILogger<GenerateMessageComponent> Logger { get; init; }

  private string _thinkingRespones = string.Empty;
  private string _properResponse = string.Empty;
  private string _userRequest = string.Empty;

  private bool _running = false;

  private OllamaGeneralModel? _selectedModel;
  private IEnumerable<OllamaGeneralModel> _models = [];

  protected override async Task OnInitializedAsync()
  {
    var models = await OllamaModelCaller.GetOllamaModelsAsync(default);
    if (models.IsFailed)
    {
      Logger.LogError(string.Join('\n', models.Errors.Select(error => error.Message)));
      return;
    }

    _models = models.Value;
    _selectedModel = models.Value.FirstOrDefault();
    StateHasChanged();
  }

  public async Task SendMessageAsync()
  {
    if (_selectedModel is null)
    {
      Console.WriteLine("Invalid, model is not selectec.");
      return;
    }

    _thinkingRespones = string.Empty;
    _properResponse = string.Empty;
    StateHasChanged();

    OllamaGenerateResponseRequest request = new OllamaGenerateDefaultResponseRequest()
    {
      Model = _selectedModel.Name,
      Prompt = _userRequest
    };

    IAsyncEnumerable<OllamaMessageResponse?> responses = OllamaGenerationCaller.GenereteAResponseAsync(request, default);
    _running = true;
    StateHasChanged();
    int i = 0;
    await foreach (OllamaMessageResponse? chunk in responses)
    {
      if (chunk is not OllamaMessageResponse response)
      {
        Logger.LogWarning(
          "Dropped chunk no: {No}",
          i++
        );

        continue;
      }

      if (response.Thinking is string thinking)
        _thinkingRespones += thinking;
      if (response.Response is string proper)
        _properResponse += proper;

      StateHasChanged();
      i++;
    }

    _running = false;
    StateHasChanged();
  }
}
