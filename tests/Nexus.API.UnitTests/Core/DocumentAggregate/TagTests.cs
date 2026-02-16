using Nexus.API.Core.Aggregates.DocumentAggregate;
using Shouldly;

namespace Nexus.API.UnitTests.Core.DocumentAggregate;

public class TagTests
{
  [Fact]
  public void Create_WithValidName_ReturnsTag()
  {
    var tag = Tag.Create("test-tag");

    tag.Name.ShouldBe("test-tag");
  }

  [Fact]
  public void Create_NormalizesToLowercase()
  {
    var tag = Tag.Create("My Tag");

    tag.Name.ShouldBe("my tag");
  }

  [Fact]
  public void Create_TrimsWhitespace()
  {
    var tag = Tag.Create("  trimmed  ");

    tag.Name.ShouldBe("trimmed");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void Create_WithEmptyOrNullName_ThrowsArgumentException(string? name)
  {
    Should.Throw<ArgumentException>(() => Tag.Create(name!));
  }

  [Fact]
  public void Create_NameExceeding50Chars_ThrowsArgumentException()
  {
    var longName = new string('a', 51);

    Should.Throw<ArgumentException>(() => Tag.Create(longName));
  }

  [Fact]
  public void Create_NameAt50Chars_Succeeds()
  {
    var name = new string('a', 50);
    var tag = Tag.Create(name);

    tag.Name.Length.ShouldBe(50);
  }

  [Fact]
  public void Create_WithColor_SetsColor()
  {
    var tag = Tag.Create("colored", "#FF0000");

    tag.Color.ShouldBe("#FF0000");
  }

  [Fact]
  public void Create_WithoutColor_ColorIsNull()
  {
    var tag = Tag.Create("no-color");

    tag.Color.ShouldBeNull();
  }

  [Fact]
  public void UpdateColor_WithValidHex_UpdatesColor()
  {
    var tag = Tag.Create("tag");

    tag.UpdateColor("#00FF00");

    tag.Color.ShouldBe("#00FF00");
  }

  [Fact]
  public void UpdateColor_WithNull_ClearsColor()
  {
    var tag = Tag.Create("tag", "#FF0000");

    tag.UpdateColor(null);

    tag.Color.ShouldBeNull();
  }

  [Fact]
  public void UpdateColor_WithInvalidHex_ThrowsException()
  {
    var tag = Tag.Create("tag");

    Should.Throw<Exception>(() => tag.UpdateColor("not-a-color"));
  }

  [Fact]
  public void UpdateColor_WithShortHex_Succeeds()
  {
    var tag = Tag.Create("tag");

    tag.UpdateColor("#FFF");

    tag.Color.ShouldBe("#FFF");
  }
}
