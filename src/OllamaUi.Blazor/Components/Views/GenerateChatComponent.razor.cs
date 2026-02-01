using System.Text;
using System.Text.Json;
using OllamaUi.Blazor.Models;
using OllamaUi.Caller.Enums;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Models;
using OllamaUi.Caller.Requests;
using OllamaUi.Caller.Responses;
using FluentResults;
using Radzen.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OllamaUi.Blazor.Components.Views;

public partial class GenerateChatComponent: ComponentBase, IDisposable
{
  [Inject]
  public required IOllamaGenerationCaller GeneratorCaller { get; init; }
  [Inject]
  public required IOllamaModelCaller ModelCaller { get; init; }
  [Inject]
  public required IJSRuntime JSRuntime { get; init; }
  [Inject]
  public required ILogger<GenerateChatComponent> Logger { get; init; }
  [Inject]
  public required PageState PageState { get; init; }

  [Parameter, EditorRequired]
  public required Guid ChatId { get; init; }
  [Parameter]
  public SetupNewChat? SetupNewChat { get; init; }

  private RadzenButton? _sendMessageButton;

  private List<OllamaChatContent> _messages = [];
  private OllamaChatContent? _currentRender = null;
  private string _userInput = string.Empty;

  private IEnumerable<OllamaGeneralModel> _models = [];
  private OllamaGeneralModel? _selectedModel = null;

  private bool _hasNewText;
  private CancellationTokenSource _cts = new();

  protected override async Task OnInitializedAsync()
  {
    _currentRender = null;
    _userInput = string.Empty;

    _ = ScheduleRenderAsync(100);

    var models = await ModelCaller.GetOllamaModelsAsync(default);
    if (models.IsFailed)
    {
      Logger.LogError("Could not load available models");
      return;
    }

    _models = models.Value;

    if (SetupNewChat is SetupNewChat setup)
      await SetupNewChatAsync(setup);
    else
      await RestoreSessionAsync();

    StateHasChanged();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      await JSRuntime.InvokeVoidAsync("bind_ctrl_enter", new object[0]);
      if (SetupNewChat is not null)
        await SendChatMessageAsync();
    }
  }

  private async Task RestoreSessionAsync()
  {
    string saveLocation = Path.Combine(PageState.ChatSaveLocation, ChatId.ToString());
    if (!Directory.Exists(saveLocation))
    {
      Logger.LogWarning(
        "Could not find location for save data, make sure you have {ChatId} within your save location",
        ChatId.ToString("d")
      );

      return;
    }

    DirectoryInfo info = new DirectoryInfo(saveLocation);
    FileInfo? manifestFile = info.GetFiles().FirstOrDefault(file => file.Name is "manifest.json");
    if (manifestFile is null)
    {
      Logger.LogWarning(
        "Could not find manifest for save data, make sure you have {ChatId} within your save location",
        ChatId.ToString("d")
      );

      return;
    }

    DirectoryInfo? messageDirctory = info.GetDirectories().FirstOrDefault(dir => dir.Name is "messages");
    if (messageDirctory is null)
    {
      Logger.LogWarning(
        "Could not find saved messages for {ChatId} within your save location",
        ChatId.ToString("d")
      );

      return;
    }

    Result<Manifest> manifest = await GetManifestFromFileStreamAsync(manifestFile.OpenRead());
    if (manifest.IsFailed)
    {
      Logger.LogError("Malformed manifest file for chat id {ChatId}", ChatId.ToString());
      return;
    }

    _selectedModel = _models.FirstOrDefault(model => model.Name == manifest.Value.OllamaModelName);

    IEnumerable<FileInfo> messageFilePaths = messageDirctory
      .GetFiles()
      .Where(file => file.Name.EndsWith(".json"));

    foreach (FileInfo messageFile in messageFilePaths)
    {
      using FileStream messageStream = messageFile.OpenRead();
      OllamaChatContent? message = await JsonSerializer.DeserializeAsync<OllamaChatContent>(messageStream);
      if (message is null)
      {
        Logger.LogError("Malformed message in \"{MessageFile}\"", messageFile.FullName);
        return;
      }

      _messages.Add(message);
    }
  }

  private async Task<Result<Manifest>> GetManifestFromFileStreamAsync(FileStream manifestStream)
  {
    JsonSerializerOptions options = new JsonSerializerOptions
    {
      AllowTrailingCommas = true,
      WriteIndented = true,
    };
    Manifest? manifest = await JsonSerializer.DeserializeAsync<Manifest>(manifestStream, options: options);

    if (manifest is null)
      return Result.Fail("Could not deserialize manifest file");

    return Result.Ok(manifest);
  }

  private async Task SetupNewChatAsync(SetupNewChat setup)
  {
    if (_sendMessageButton is null)
      return;

    _selectedModel = _models.FirstOrDefault(model => model.Name == setup.ModelName);
    _userInput = setup.Prompt;

    await Task.CompletedTask;
  }

  private StringBuilder _thinkingBuilder = new();
  private StringBuilder _responseBuilder = new();

  private async Task SendChatMessageAsync()
  {
    if (_selectedModel is null)
    {
      Logger.LogError("Can not send message to an unknown model");
      return;
    }

    IAsyncEnumerable<OllamaChatResponse?> chunks = await SendChatRequestAsync(_selectedModel.Name);
    OllamaChatContent content = await ConsumeMessageChunksAsync(chunks);

    int no = _messages.Count();
    await WriteResponseToSaveLocationAsync(content, no);

    _currentRender = null;
    StateHasChanged();
  }

  private Task<IAsyncEnumerable<OllamaChatResponse?>> SendChatRequestAsync(string modelName)
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

    OllamaGenerateChatRequest request = new OllamaGenerateChatRequest()
    {
      Model = modelName,
      Messages = messages,
      Options = new()
      {
        ContextSize = 16384
      }
    };

    IAsyncEnumerable<OllamaChatResponse?> chunks = GeneratorCaller.GenerateChatMessageAsync(request, default);

    return Task.FromResult(chunks);
  }

  private async Task<OllamaChatContent> ConsumeMessageChunksAsync(IAsyncEnumerable<OllamaChatResponse?> chunks)
  {
    OllamaChatContent nextResponse = new OllamaChatContent
    {
      Prompt = _userInput,
      ThinkingText = string.Empty,
      Response = string.Empty
    };
    _currentRender = nextResponse;
    _userInput = string.Empty;

    int i = 0;
    await foreach (var chunk in chunks)
    {
      if (chunk is null)
      {
        Logger.LogWarning(
          "[Origin: {ClassName}.{MethodName}] Dropped chat chunk no: {No}",
          nameof(GenerateChatComponent),
          nameof(SendChatMessageAsync),
          i++
        );

        continue;
      }

      if (chunk.Message.Thinking is not null)
      {
        _thinkingBuilder.Append(chunk.Message.Thinking);
        nextResponse.ActiveThinking = true;
      }
      if (chunk.Message.Content is not null)
      {
        _responseBuilder.Append(chunk.Message.Content);
        nextResponse.ActiveThinking = false;
      }

      _hasNewText = true;
      StateHasChanged();
    }

    nextResponse.Done = true;
    _messages.Add(nextResponse);

    return nextResponse;
  }

  private async Task WriteResponseToSaveLocationAsync(OllamaChatContent content, int number)
  {
    string saveLocation = Path.Combine(PageState.ChatSaveLocation, ChatId.ToString("d"), "messages");
    if (!Directory.Exists(saveLocation))
    {
      Logger.LogWarning(
        "Cannot find save location for chat, please make sure a location exists for id: {ChatId}",
        ChatId.ToString("d")
      );

      return;
    }

    saveLocation = Path.Combine(saveLocation, number.ToString() + ".json");

    JsonSerializerOptions options = new JsonSerializerOptions
    {
      WriteIndented = true,
      AllowTrailingCommas = true
    };

    var fileContent = JsonSerializer.Serialize(content, options: options);

    try
    {
      await File.WriteAllTextAsync(saveLocation, fileContent);
    }
    catch (Exception e)
    {
      Logger.LogCritical(
        e,
        "Failed to write save file {FileCount} for chat id {ChatId}, please insert into appropriate file: \"{Content}\"",
        number,
        ChatId.ToString("d"),
        fileContent
      );

      return;
    }
  }

  private async Task ScheduleRenderAsync(int delayMs)
  {
    while (!_cts.IsCancellationRequested)
    {
      if (_currentRender is not null && _hasNewText)
      {
        _currentRender.ThinkingText += _thinkingBuilder.ToString();
        _currentRender.Response += _responseBuilder.ToString();

        _thinkingBuilder.Clear();
        _responseBuilder.Clear();

        _hasNewText = false;

        await InvokeAsync(StateHasChanged);
      }

      await Task.Delay(delayMs);
    }
  }

  private string GetThinkingClass()
  {
    return string.Empty;
  }

  public void Dispose()
  {
    _cts.Cancel();
    _cts.Dispose();
  }
}

