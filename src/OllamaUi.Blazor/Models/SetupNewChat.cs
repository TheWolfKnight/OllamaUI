
namespace OllamaUi.Blazor.Models;

public class SetupNewChat
{
  public required string Prompt { get; init;}
  public required Guid ChatId { get; init; }
  public required string ModelName { get; init; }
}
