using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace OllamaUi.Caller.Services;

public class OllamaModelCaller: IOllamaModelCaller
{
  private record OllamaModelResponse<TOllamaModel>
  where TOllamaModel: OllamaModel
  {
    [JsonPropertyName("models")]
    public required IEnumerable<TOllamaModel>? Models { get; init; }
  }

  private const string _URL_BASE = "http://localhost:11434";
  private readonly HttpClient _httpClient;
  private readonly ILogger<OllamaModelCaller> _logger;

  public OllamaModelCaller(ILogger<OllamaModelCaller> logger)
  {
    _httpClient = new HttpClient();
    _httpClient.BaseAddress = new(_URL_BASE);

    _logger = logger;
  }

  public async Task<Result<IEnumerable<OllamaGeneralModel>>> GetOllamaModelsAsync(CancellationToken cancellationToken)
  {
    const string URL = "api/tags";
    try
    {
      HttpResponseMessage response = await _httpClient.GetAsync(URL, cancellationToken);

      if (response.StatusCode is not HttpStatusCode.OK)
      {
        string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
          "[Origin: {ClassName}.{MethodName}] Received invalid status code from {EndpointUrl} endpoint: {StatusCode}:{Message}",
          nameof(OllamaModelCaller),
          nameof(GetOllamaModelsAsync),
          URL,
          response.StatusCode.ToString(),
          errorMessage
        );

         return Result.Fail("Ollama returned invalid status code when fetching models");
      }

      OllamaModelResponse<OllamaGeneralModel>? ollamaModels = await response.Content.ReadFromJsonAsync<OllamaModelResponse<OllamaGeneralModel>>(cancellationToken);
      if (ollamaModels is not { Models: not null } valid || !valid.Models.Any())
        return Result.Fail("No Ollama models were found");

      return Result.Ok(ollamaModels.Models);
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
      _logger.LogCritical(
        e,
        "[Origin: {ClassName}.{MethodName}] Could not fetch Ollama Models due to internal error",
        nameof(OllamaModelCaller),
        nameof(GetOllamaModelsAsync)
      );

      return Result.Fail("Unable to find Ollama models due to internal error");
    }
  }

  public async Task<Result<IEnumerable<OllamaRunningModel>>> GetRunningOllamaModelsAsync(CancellationToken cancellationToken)
  {
    const string URL = "api/ps";
    try
    {
      HttpResponseMessage response = await _httpClient.GetAsync(URL);

      if (response.StatusCode is not HttpStatusCode.OK)
      {
        string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
          "[Origin: {ClassName}.{MethodName}] Received invalid status code from {EndpointUrl} endpoint: {StatusCode}:{Message}",
          nameof(OllamaModelCaller),
          nameof(GetOllamaModelsAsync),
          URL,
          response.StatusCode.ToString(),
          errorMessage
        );

         return Result.Fail("Ollama returned invalid status code when fetching running models");
      }

      OllamaModelResponse<OllamaRunningModel>? runningOllamaModels = await response
        .Content
        .ReadFromJsonAsync<OllamaModelResponse<OllamaRunningModel>>(cancellationToken);
      if (runningOllamaModels is not { Models: not null } valid || !valid.Models.Any())
        return Result.Fail("No running Ollama models");

      return Result.Ok(runningOllamaModels.Models);
    }
    catch (Exception e)
    {
      _logger.LogCritical(
        e,
        "[Origin: {ClassName}.{MethodName}] Could not fetch running Ollama Models due to internal error",
        nameof(OllamaModelCaller),
        nameof(GetRunningOllamaModelsAsync)
      );

      return Result.Fail("Unable to find running Ollama models due to internal error");
    }
  }
}
