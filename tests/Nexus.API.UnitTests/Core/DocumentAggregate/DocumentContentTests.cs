using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.DocumentAggregate;

public class DocumentContentTests
{
  [Fact]
  public void Create_WithHtml_StripsToPlainText()
  {
    var content = DocumentContent.Create("<p>Hello <strong>world</strong></p>");

    content.PlainText.ShouldBe("Hello world");
  }

  [Fact]
  public void Create_CountsWords()
  {
    var content = DocumentContent.Create("<p>one two three four five</p>");

    content.WordCount.ShouldBe(5);
  }

  [Fact]
  public void Create_EmptyContent_ReturnsZeroWordCount()
  {
    var content = DocumentContent.Create("");

    content.WordCount.ShouldBe(0);
    content.PlainText.ShouldBe(string.Empty);
  }

  [Fact]
  public void Create_PreservesRichText()
  {
    var html = "<h1>Title</h1><p>Paragraph</p>";
    var content = DocumentContent.Create(html);

    content.RichText.ShouldBe(html);
  }

  [Fact]
  public void Create_WithNull_ThrowsArgumentException()
  {
    Should.Throw<ArgumentNullException>(() => DocumentContent.Create(null!));
  }
}
