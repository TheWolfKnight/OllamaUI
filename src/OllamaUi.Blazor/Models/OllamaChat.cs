using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace OllamaUi.Blazor.Models;

public class OllamaChat
{
  public Guid ChatId { get; init; }
  public string ModelName { get; init; }

  public string Name { get; set; } = string.Empty;

  public List<OllamaChatContent> Messages { get; init; }

  public OllamaChat(string modelName)
  {
    ChatId = Guid.NewGuid();
    ModelName = modelName;
    Messages = new();
  }

  public async Task<Result> SaveChatToFileAsync()
  {
    throw new NotImplementedException();
  }

  public static async Task<Result<OllamaChat>> LoadChatFromSaveFileAsync(string chatSavePath, ILogger? logger = null)
  {
    if (!File.Exists(chatSavePath) || chatSavePath.EndsWith(".json"))
      return Result.Fail($"Could not find a valid file at path '{chatSavePath}'. The file must exist and be JSON");

    try
    {
      FileStream stream = File.OpenRead(chatSavePath);
      OllamaChat? chat = await JsonSerializer.DeserializeAsync<OllamaChat>(stream);
      if (chat is null)
        return Result.Fail($"The save file '{chatSavePath}' exists, and was oppened, but the data did not result in a valid chat");

      return Result.Ok(chat);
    }
    catch (Exception e)
    {
      if (logger is not null)
        logger.LogError(
          e,
          "[Origin: {ClassName}.{MethodName}] An error happend while trying to read the file {FilePath}",
          nameof(OllamaChat),
          nameof(LoadChatFromSaveFileAsync),
          chatSavePath
        );

      return Result.Fail($"An error occured while trying to read the save file '{chatSavePath}'");
    }
  }
}
