using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Requests;
using OllamaUi.Caller.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace OllamaUi.Blazor.Pages;

public partial class Home: ComponentBase
{
  [Inject]
  public required IOllamaGenerationCaller OllamaGenerationCaller { get; init; }
  [Inject]
  public required ILogger<Home> Logger { get; init; }

  private string _thinkingRespones = string.Empty;
  private string _properResponse = string.Empty;

  private string _userRequest = string.Empty;

  public async Task SendMessageAsync()
  {
    _thinkingRespones = string.Empty;
    _properResponse = string.Empty;
    StateHasChanged();

    OllamaGenerateResponseRequest request = new OllamaGenerateDefaultResponseRequest()
    {
      Model = "gpt-oss:20b",
      Prompt = _userRequest
    };

    IAsyncEnumerable<OllamaMessageResponse?> responses = OllamaGenerationCaller.GenereteAResponseAsync(request, default);
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
  }
}
