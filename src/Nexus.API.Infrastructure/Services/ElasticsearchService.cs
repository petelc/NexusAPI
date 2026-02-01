using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.Models;

namespace Nexus.Infrastructure.Services;

/// <summary>
/// Elasticsearch service for full-text search and analytics.
/// Implements the ISearchService interface from the Core layer.
/// </summary>
public class ElasticsearchService : ISearchService
{
  private readonly ElasticsearchClient _client;
  private readonly ILogger<ElasticsearchService> _logger;
  private const string DocumentsIndexName = "nexus-documents";
  private const string DiagramsIndexName = "nexus-diagrams";
  private const string SnippetsIndexName = "nexus-snippets";

  public ElasticsearchService(
    ILogger<ElasticsearchService> logger,
    IConfiguration configuration)
  {
    _logger = logger;

    var uri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
    var username = configuration["Elasticsearch:Username"];
    var password = configuration["Elasticsearch:Password"];

    var settings = new ElasticsearchClientSettings(new Uri(uri))
      .DefaultIndex(DocumentsIndexName)
      .RequestTimeout(TimeSpan.FromSeconds(30));

    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
    {
      settings.Authentication(new BasicAuthentication(username, password));
    }

    _client = new ElasticsearchClient(settings);
  }

  /// <summary>
  /// Initialize Elasticsearch indexes
  /// </summary>
  public async Task InitializeIndexesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // Create documents index
      await CreateDocumentIndexAsync(cancellationToken);

      // Create diagrams index
      await CreateDiagramIndexAsync(cancellationToken);

      // Create snippets index
      await CreateSnippetIndexAsync(cancellationToken);

      _logger.LogInformation("Elasticsearch indexes initialized successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error initializing Elasticsearch indexes");
      throw;
    }
  }

  /// <summary>
  /// Index a document for searching
  /// </summary>
  public async Task IndexDocumentAsync(
    Guid documentId,
    string title,
    string content,
    IEnumerable<string> tags,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var searchDocument = new DocumentSearchModel
      {
        DocumentId = documentId,
        Title = title,
        Content = content,
        Tags = tags.ToList(),
        UserId = userId,
        IndexedAt = DateTime.UtcNow
      };

      var response = await _client.IndexAsync(
        searchDocument,
        idx => idx.Index(DocumentsIndexName).Id(documentId.ToString()),
        cancellationToken);

      if (!response.IsValidResponse)
      {
        _logger.LogWarning("Failed to index document {DocumentId}: {Error}",
          documentId, response.DebugInformation);
      }
      else
      {
        _logger.LogInformation("Document {DocumentId} indexed successfully", documentId);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error indexing document {DocumentId}", documentId);
      throw;
    }
  }

  /// <summary>
  /// Search documents with full-text search
  /// </summary>
  public async Task<SearchResults> SearchDocumentsAsync(
    string query,
    int page = 1,
    int pageSize = 20,
    IEnumerable<string>? tags = null,
    Guid? userId = null,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var from = (page - 1) * pageSize;

      var searchResponse = await _client.SearchAsync<DocumentSearchModel>(s => s
        .Index(DocumentsIndexName)
        .From(from)
        .Size(pageSize)
        .Query(q => q
          .Bool(b =>
          {
            var must = new List<Action<QueryDescriptor<DocumentSearchModel>>>();

            // Full-text search on title and content
            if (!string.IsNullOrWhiteSpace(query))
            {
              must.Add(m => m.MultiMatch(mm => mm
                .Query(query)
                .Fields(new[] { "title^2", "content" })
                .Fuzziness(new Fuzziness("AUTO"))
              ));
            }

            // Filter by tags
            if (tags != null && tags.Any())
            {
              must.Add(m => m.Terms(t => t
                .Field(f => f.Tags)
                .Terms(new TermsQueryField(tags.Select(tag => FieldValue.String(tag)).ToArray()))
              ));
            }

            // Filter by user
            if (userId.HasValue)
            {
              must.Add(m => m.Term(t => t
                .Field(f => f.UserId)
                .Value(userId.Value)
              ));
            }

            return b.Must(must.ToArray());
          })
        )
        .Highlight(h => h
          .Fields(f => f
            .Add(fd => fd.Title, new HighlightField())
            .Add(fd => fd.Content, new HighlightField { FragmentSize = 150, NumberOfFragments = 3 })
          )
        )
      , cancellationToken);

      if (!searchResponse.IsValidResponse)
      {
        _logger.LogWarning("Search failed: {Error}", searchResponse.DebugInformation);
        return new SearchResults { Results = new List<SearchResult>(), TotalCount = 0 };
      }

      var results = searchResponse.Documents.Select((doc, index) =>
      {
        var hit = searchResponse.Hits.ElementAt(index);
        return new SearchResult
        {
          DocumentId = doc.DocumentId,
          Title = doc.Title,
          Excerpt = GetHighlightedContent(hit.Highlight, doc.Content),
          Score = hit.Score ?? 0,
          Tags = doc.Tags,
          IndexedAt = doc.IndexedAt
        };
      }).ToList();

      return new SearchResults
      {
        Results = results,
        TotalCount = (int)(searchResponse.Total),
        Page = page,
        PageSize = pageSize
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error searching documents with query: {Query}", query);
      throw;
    }
  }

  /// <summary>
  /// Delete a document from the search index
  /// </summary>
  public async Task DeleteDocumentAsync(
    Guid documentId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var response = await _client.DeleteAsync(
        DocumentsIndexName,
        documentId.ToString(),
        cancellationToken);

      if (!response.IsValidResponse)
      {
        _logger.LogWarning("Failed to delete document {DocumentId} from index: {Error}",
          documentId, response.DebugInformation);
      }
      else
      {
        _logger.LogInformation("Document {DocumentId} deleted from index successfully", documentId);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting document {DocumentId} from index", documentId);
      throw;
    }
  }

  /// <summary>
  /// Get search suggestions/autocomplete
  /// </summary>
  public async Task<IEnumerable<string>> GetSuggestionsAsync(
    string prefix,
    int maxSuggestions = 10,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var searchResponse = await _client.SearchAsync<DocumentSearchModel>(s => s
        .Index(DocumentsIndexName)
        .Size(maxSuggestions)
        .Query(q => q
          .MatchPhrase(mp => mp
            .Field(f => f.Title)
            .Query(prefix)
          )
        )
        .Source(src => src.Includes(i => i.Field(f => f.Title)))
      , cancellationToken);

      if (!searchResponse.IsValidResponse)
      {
        return Enumerable.Empty<string>();
      }

      return searchResponse.Documents.Select(d => d.Title).Distinct();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting suggestions for prefix: {Prefix}", prefix);
      return Enumerable.Empty<string>();
    }
  }

  private async Task CreateDocumentIndexAsync(CancellationToken cancellationToken)
  {
    var indexExists = await _client.Indices.ExistsAsync(DocumentsIndexName, cancellationToken);

    if (indexExists.Exists)
    {
      _logger.LogInformation("Documents index already exists");
      return;
    }

    var response = await _client.Indices.CreateAsync<DocumentSearchModel>(
      DocumentsIndexName,
      cancellationToken);

    if (!response.IsValidResponse)
    {
      _logger.LogWarning("Failed to create documents index: {Error}", response.DebugInformation);
    }
  }

  private async Task CreateDiagramIndexAsync(CancellationToken cancellationToken)
  {
    var indexExists = await _client.Indices.ExistsAsync(DiagramsIndexName, cancellationToken);

    if (indexExists.Exists)
    {
      _logger.LogInformation("Diagrams index already exists");
      return;
    }

    var response = await _client.Indices.CreateAsync(DiagramsIndexName, cancellationToken);

    if (!response.IsValidResponse)
    {
      _logger.LogWarning("Failed to create diagrams index: {Error}", response.DebugInformation);
    }
  }

  private async Task CreateSnippetIndexAsync(CancellationToken cancellationToken)
  {
    var indexExists = await _client.Indices.ExistsAsync(SnippetsIndexName, cancellationToken);

    if (indexExists.Exists)
    {
      _logger.LogInformation("Snippets index already exists");
      return;
    }

    var response = await _client.Indices.CreateAsync(SnippetsIndexName, cancellationToken);

    if (!response.IsValidResponse)
    {
      _logger.LogWarning("Failed to create snippets index: {Error}", response.DebugInformation);
    }
  }

  private string GetHighlightedContent(
    IReadOnlyDictionary<string, IReadOnlyCollection<string>>? highlights,
    string originalContent)
  {
    if (highlights == null || !highlights.Any())
    {
      return originalContent.Length > 200 ? originalContent.Substring(0, 200) + "..." : originalContent;
    }

    if (highlights.TryGetValue("content", out var contentHighlights) && contentHighlights.Any())
    {
      return string.Join(" ... ", contentHighlights);
    }

    if (highlights.TryGetValue("title", out var titleHighlights) && titleHighlights.Any())
    {
      return titleHighlights.First();
    }

    return originalContent.Length > 200 ? originalContent.Substring(0, 200) + "..." : originalContent;
  }
}

/// <summary>
/// Document model for Elasticsearch indexing (Infrastructure-specific)
/// </summary>
public class DocumentSearchModel
{
  public Guid DocumentId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public List<string> Tags { get; set; } = new();
  public Guid UserId { get; set; }
  public DateTime IndexedAt { get; set; }
}
