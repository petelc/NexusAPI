using Ardalis.GuardClauses;
using Traxs.SharedKernel;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Value object representing a programming language with metadata
/// Immutable - language details don't change per snippet
/// </summary>
public class ProgrammingLanguage : ValueObject
{
  public string Name { get; private set; }
  public string FileExtension { get; private set; }
  public string? Version { get; private set; }

  private ProgrammingLanguage(string name, string fileExtension, string? version = null)
  {
    Name = name;
    FileExtension = fileExtension;
    Version = version;
  }

  public static ProgrammingLanguage Create(string name, string fileExtension, string? version = null)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.NullOrWhiteSpace(fileExtension, nameof(fileExtension));

    // Normalize inputs
    name = name.Trim();
    fileExtension = fileExtension.Trim().TrimStart('.');

    return new ProgrammingLanguage(name, fileExtension, version?.Trim());
  }

  // Common languages factory methods
  public static ProgrammingLanguage CSharp(string? version = null) => 
    Create("C#", "cs", version);

  public static ProgrammingLanguage JavaScript(string? version = null) => 
    Create("JavaScript", "js", version);

  public static ProgrammingLanguage TypeScript(string? version = null) => 
    Create("TypeScript", "ts", version);

  public static ProgrammingLanguage Python(string? version = null) => 
    Create("Python", "py", version);

  public static ProgrammingLanguage Java(string? version = null) => 
    Create("Java", "java", version);

  public static ProgrammingLanguage SQL(string? version = null) => 
    Create("SQL", "sql", version);

  public static ProgrammingLanguage Bash(string? version = null) => 
    Create("Bash", "sh", version);

  public static ProgrammingLanguage PowerShell(string? version = null) => 
    Create("PowerShell", "ps1", version);

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Name;
    yield return FileExtension;
    yield return Version ?? string.Empty;
  }

  public override string ToString() => 
    Version != null ? $"{Name} ({Version})" : Name;
}
