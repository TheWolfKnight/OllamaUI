using System.Text.Json.Serialization;

namespace OllamaUi.Blazor.Models;

[JsonDerivedType(typeof(Manifest_V1), nameof(Manifest_V1))]
public abstract class Manifest
{
  public abstract string GetManifestVersion { get; }
  public required Guid ChatId { get; init; }

  public required string OllamaModelName { get; init; }
  public required string DisplayName { get; init; }

  public required DateTime CreationDate { get; init; }
}
