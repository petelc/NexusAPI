using Ardalis.GuardClauses;
using Traxs.SharedKernel;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Value object representing document content with both rich text and plain text versions
/// </summary>
public class DocumentContent : ValueObject
{
    public string RichText { get; private set; }
    public string PlainText { get; private set; }
    public int WordCount { get; private set; }

    private DocumentContent(string richText, string plainText, int wordCount)
    {
        RichText = richText;
        PlainText = plainText;
        WordCount = wordCount;
    }

    public static DocumentContent Create(string richText)
    {
        Guard.Against.Null(richText, nameof(richText));

        var plainText = StripHtml(richText);
        var wordCount = CountWords(plainText);

        return new DocumentContent(richText, plainText, wordCount);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Simple HTML stripping - in production, use a proper library like HtmlAgilityPack
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(text).Trim();
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RichText;
    }
}
