using System.Collections.Generic;
using System.Threading;
using OllamaUi.Caller.Requests;
using OllamaUi.Caller.Responses;

namespace OllamaUi.Caller.Interfaces;

public interface IOllamaGenerationCaller
{
  IAsyncEnumerable<OllamaMessageResponse?> GenereteAResponseAsync(OllamaGenerateResponseRequest request, CancellationToken cancellationToken);

  IAsyncEnumerable<OllamaChatResponse?> GenerateChatMessageAsync(OllamaGenerateChatRequest request, CancellationToken cancellationToken);
}
