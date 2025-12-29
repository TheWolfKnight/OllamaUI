using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Requests;
using OllamaUi.Caller.Responses;
using Microsoft.Extensions.Logging;

namespace OllamaUi.Caller.Services;

public class OllamaGenerationCaller: IOllamaGenerationCaller
{
  private const string BASE_URL = "http://localhost:11434";
  private readonly HttpClient _httpClient;
  private readonly ILogger<OllamaGenerationCaller> _logger;

  public OllamaGenerationCaller(ILogger<OllamaGenerationCaller> logger)
  {
    _httpClient = new HttpClient();
    _httpClient.BaseAddress = new(BASE_URL);

    _logger = logger;
  }

  public async IAsyncEnumerable<OllamaMessageResponse?> GenereteAResponseAsync(OllamaGenerateResponseRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    const string URL = "/api/generate";
    HttpResponseMessage response;
    try
    {
      Uri uri = new Uri(_httpClient.BaseAddress ?? new (""), URL);
      HttpRequestMessage content = new HttpRequestMessage(HttpMethod.Post, uri)
      {
        Content = JsonContent.Create(request)
      };

      response = await _httpClient.SendAsync(
        content,
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken
      );
    }
    catch (Exception e)
    {
      _logger.LogCritical(
        e,
        "[Origin: {ClassName}.{MethodName}] Could not get response message from Ollama due to internal error",
        nameof(OllamaGenerationCaller),
        nameof(GenereteAResponseAsync)
      );

      throw new InvalidOperationException("Due to an internal error, the response could not be received");
    }

    if (response.StatusCode is not HttpStatusCode.OK)
    {
      string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
      _logger.LogError(
        "[Origin: {ClassName}.{MethodName}] Received invalid status code from {EndpointUrl} endpoint: {StatusCode}:{Message}",
        nameof(OllamaGenerationCaller),
        nameof(GenereteAResponseAsync),
        URL,
        response.StatusCode.ToString(),
        errorMessage
      );

      throw new InvalidOperationException("Could not get response due to invalid status code");
    }

    Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
    StreamReader reader = new StreamReader(contentStream);
    try
    {
      int i = 0;
      _logger.LogInformation("Begin read");
      while (await reader.ReadLineAsync(cancellationToken) is string line)
      {
        OllamaMessageResponse? chunk = JsonSerializer.Deserialize<OllamaMessageResponse>(line);

        _logger.LogInformation(
          "[DEBUG: {Time}] chunk no: {No} received data:\nchunk is not null: {ChunkExists}\nThinking: \"{ChunkThink}\",\nResponse: \"{ChunkResponse}\"",
          DateTime.Now.ToShortTimeString(),
          i++,
          chunk is not null,
          chunk?.Thinking,
          chunk?.Response
        );

        yield return chunk;
      }
    }
    finally
    {
      reader.Close();
      contentStream.Close();
    }
  }

  public async IAsyncEnumerable<OllamaChatResponse?> GenerateChatMessageAsync(OllamaGenerateChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    const string URL = "/api/chat";
    HttpResponseMessage response;
    try
    {
      Uri uri = new Uri(_httpClient.BaseAddress ?? new Uri(""), URL);
      HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, uri)
      {
        Content = JsonContent.Create(request)
      };

      response = await _httpClient.SendAsync(
        message,
        HttpCompletionOption.ResponseHeadersRead,
        cancellationToken
      );
    }
    catch (Exception e)
    {
      _logger.LogCritical(
        e,
        "[Origin: {ClassName}.{MethodName}] Could not get chat message from Ollama due to internal error",
        nameof(OllamaGenerationCaller),
        nameof(GenerateChatMessageAsync)
      );

      throw new InvalidOperationException("Due to an internal error, the chat could not be received");
    }

    if (response.StatusCode is not HttpStatusCode.OK)
    {
      string errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
      _logger.LogError(
        "[Origin: {ClassName}.{MethodName}] Received invalid status code from {EndpointUrl} endpoint: {StatusCode}:{Message}",
        nameof(OllamaGenerationCaller),
        nameof(GenerateChatMessageAsync),
        URL,
        response.StatusCode.ToString(),
        errorMessage
      );

      throw new InvalidOperationException("Could not get response due to invalid error code");
    }

    int i = 0;
    Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
    StreamReader reader = new StreamReader(contentStream);

    try
    {
      while (await reader.ReadLineAsync(cancellationToken) is string line)
      {
        OllamaChatResponse? chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line);

        _logger.LogInformation(
          "[DEBUG: {Time}] chunk no: {No} received data:\nchunk is not null: {ChunkExists}\nThinking: \"{ChunkThink}\",\nResponse: \"{ChunkResponse}\"",
          DateTime.Now.ToShortTimeString(),
          i++,
          chunk is not null,
          chunk?.Message.Thinking,
          chunk?.Message.Content
        );

        yield return chunk;
      }
    }
    finally
    {
      reader.Close();
      contentStream.Close();
    }
  }
}
