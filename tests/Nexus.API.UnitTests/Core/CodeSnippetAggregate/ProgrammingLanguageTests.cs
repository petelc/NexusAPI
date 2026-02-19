using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.CodeSnippetAggregate;

public class ProgrammingLanguageTests
{
  // ─── Create ────────────────────────────────────────────────────────

  [Fact]
  public void Create_WithValidInputs_ReturnsLanguage()
  {
    var lang = ProgrammingLanguage.Create("Python", "py");

    lang.Name.ShouldBe("Python");
    lang.FileExtension.ShouldBe("py");
    lang.Version.ShouldBeNull();
  }

  [Fact]
  public void Create_WithVersion_SetsVersion()
  {
    var lang = ProgrammingLanguage.Create("Python", "py", "3.11");

    lang.Version.ShouldBe("3.11");
  }

  [Fact]
  public void Create_StripsLeadingDotFromExtension()
  {
    var lang = ProgrammingLanguage.Create("C#", ".cs");

    lang.FileExtension.ShouldBe("cs");
  }

  [Fact]
  public void Create_TrimsWhitespace()
  {
    var lang = ProgrammingLanguage.Create("  JavaScript  ", "  js  ");

    lang.Name.ShouldBe("JavaScript");
    lang.FileExtension.ShouldBe("js");
  }

  [Fact]
  public void Create_WithNullName_ThrowsException()
  {
    Should.Throw<Exception>(() =>
      ProgrammingLanguage.Create(null!, "js"));
  }

  [Fact]
  public void Create_WithWhitespaceName_ThrowsException()
  {
    Should.Throw<Exception>(() =>
      ProgrammingLanguage.Create("   ", "js"));
  }

  [Fact]
  public void Create_WithNullExtension_ThrowsException()
  {
    Should.Throw<Exception>(() =>
      ProgrammingLanguage.Create("JavaScript", null!));
  }

  // ─── Factory Methods ───────────────────────────────────────────────

  [Fact]
  public void CSharp_ReturnsCorrectLanguage()
  {
    var lang = ProgrammingLanguage.CSharp();

    lang.Name.ShouldBe("C#");
    lang.FileExtension.ShouldBe("cs");
  }

  [Fact]
  public void JavaScript_ReturnsCorrectLanguage()
  {
    var lang = ProgrammingLanguage.JavaScript();

    lang.Name.ShouldBe("JavaScript");
    lang.FileExtension.ShouldBe("js");
  }

  [Fact]
  public void TypeScript_ReturnsCorrectLanguage()
  {
    var lang = ProgrammingLanguage.TypeScript();

    lang.Name.ShouldBe("TypeScript");
    lang.FileExtension.ShouldBe("ts");
  }

  [Fact]
  public void Python_ReturnsCorrectLanguage()
  {
    var lang = ProgrammingLanguage.Python();

    lang.Name.ShouldBe("Python");
    lang.FileExtension.ShouldBe("py");
  }

  [Fact]
  public void SQL_ReturnsCorrectLanguage()
  {
    var lang = ProgrammingLanguage.SQL();

    lang.Name.ShouldBe("SQL");
    lang.FileExtension.ShouldBe("sql");
  }

  [Fact]
  public void Bash_ReturnsCorrectLanguage()
  {
    var lang = ProgrammingLanguage.Bash();

    lang.Name.ShouldBe("Bash");
    lang.FileExtension.ShouldBe("sh");
  }

  [Fact]
  public void CSharp_WithVersion_SetsVersion()
  {
    var lang = ProgrammingLanguage.CSharp("12.0");

    lang.Version.ShouldBe("12.0");
  }

  // ─── Equality ──────────────────────────────────────────────────────

  [Fact]
  public void Equality_SameValues_Equal()
  {
    var lang1 = ProgrammingLanguage.Create("C#", "cs", "12.0");
    var lang2 = ProgrammingLanguage.Create("C#", "cs", "12.0");

    lang1.ShouldBe(lang2);
  }

  [Fact]
  public void Equality_DifferentName_NotEqual()
  {
    var lang1 = ProgrammingLanguage.Create("C#", "cs");
    var lang2 = ProgrammingLanguage.Create("Java", "java");

    lang1.ShouldNotBe(lang2);
  }

  [Fact]
  public void Equality_DifferentVersion_NotEqual()
  {
    var lang1 = ProgrammingLanguage.Create("Python", "py", "3.10");
    var lang2 = ProgrammingLanguage.Create("Python", "py", "3.11");

    lang1.ShouldNotBe(lang2);
  }

  // ─── ToString ──────────────────────────────────────────────────────

  [Fact]
  public void ToString_WithVersion_IncludesVersion()
  {
    var lang = ProgrammingLanguage.Create("Python", "py", "3.11");

    lang.ToString().ShouldBe("Python (3.11)");
  }

  [Fact]
  public void ToString_WithoutVersion_JustName()
  {
    var lang = ProgrammingLanguage.Create("SQL", "sql");

    lang.ToString().ShouldBe("SQL");
  }
}
