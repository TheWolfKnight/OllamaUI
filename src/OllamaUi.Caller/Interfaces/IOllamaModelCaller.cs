using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OllamaUi.Caller.Models;
using FluentResults;

namespace OllamaUi.Caller.Interfaces;

public interface IOllamaModelCaller
{
  Task<Result<IEnumerable<OllamaGeneralModel>>> GetOllamaModelsAsync(CancellationToken cancellationToken);
  Task<Result<IEnumerable<OllamaRunningModel>>> GetRunningOllamaModelsAsync(CancellationToken cancellationToken);
}
