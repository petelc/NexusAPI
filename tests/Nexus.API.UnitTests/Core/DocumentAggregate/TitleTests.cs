using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.DocumentAggregate;

public class TitleTests
{
  [Fact]
  public void Create_WithValidTitle_ReturnsTitle()
  {
    var title = Title.Create("Test Title");

    title.Value.ShouldBe("Test Title");
  }

  [Fact]
  public void Create_TrimsWhitespace()
  {
    var title = Title.Create("  Trimmed  ");

    title.Value.ShouldBe("Trimmed");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void Create_WithEmptyOrNull_ThrowsException(string? value)
  {
    Should.Throw<Exception>(() => Title.Create(value!));
  }

  [Fact]
  public void Create_ExceedingMaxLength_ThrowsException()
  {
    var longTitle = new string('a', 201);

    Should.Throw<Exception>(() => Title.Create(longTitle));
  }

  [Fact]
  public void Create_AtMaxLength_Succeeds()
  {
    var title = Title.Create(new string('a', 200));

    title.Value.Length.ShouldBe(200);
  }

  [Fact]
  public void ImplicitConversion_ReturnsStringValue()
  {
    var title = Title.Create("My Title");

    string result = title;

    result.ShouldBe("My Title");
  }

  [Fact]
  public void Equality_SameValue_AreEqual()
  {
    var title1 = Title.Create("Same");
    var title2 = Title.Create("Same");

    title1.ShouldBe(title2);
  }
}
