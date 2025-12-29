using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaUi.Blazor.Models;
using OllamaUi.Caller.Enums;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Requests;
using OllamaUi.Caller.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace OllamaUi.Blazor.Pages;

public partial class GenerateChat: ComponentBase
{
  [Inject]
  public required IOllamaGenerationCaller GeneratorCaller { get; init; }
  [Inject]
  public required ILogger<GenerateChat> Logger { get; init; }

  private List<OllamaChatContent> _messages = [];
  private string _userInput = string.Empty;

  private async Task SendChatMessageAsync()
  {
    List<OllamaChatRequestMessage> messages = _messages
      .Select(message => new OllamaChatRequestMessage[2] {
        new OllamaChatRequestMessage
        {
          Role = OllamaChatMessageRole.User,
          Content = message.Prompt
        },
        new OllamaChatRequestMessage
        {
          Role = OllamaChatMessageRole.Assistant,
          Content = message.Response
        }
      })
      .SelectMany(message => message)
      .Append(
        new OllamaChatRequestMessage
        {
          Role = OllamaChatMessageRole.User,
          Content = _userInput
        }
      )
      .ToList();

    OllamaChatContent nextResponse = new OllamaChatContent
    {
      Prompt = _userInput,
      ThinkingText = string.Empty,
      Response = string.Empty
    };
    _messages.Add(nextResponse);

    _userInput = string.Empty;

    OllamaGenerateChatRequest request = new OllamaGenerateChatRequest()
    {
      Model = "gpt-oss:20b",
      Messages = messages
    };

    IAsyncEnumerable<OllamaChatResponse?> chunks = GeneratorCaller.GenerateChatMessageAsync(request, default);

    int i = 0;
    await foreach (var chunk in chunks)
    {
      if (chunk is null)
      {
        Logger.LogWarning(
          "[Origin: {ClassName}.{MethodName}] Dropped chat chunk no: {No}",
          nameof(GenerateChat),
          nameof(SendChatMessageAsync),
          i++
        );

        continue;
      }

      if (chunk.Message.Thinking is not null)
        nextResponse.ThinkingText += chunk.Message.Thinking;
      if (chunk.Message.Content is not null)
        nextResponse.Response += chunk.Message.Content;

      StateHasChanged();
    }
  }
}

