using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Nexus.API.Core.Entities;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Elasticsearch implementation of ISearchService.
/// Uses Elastic.Clients.Elasticsearch v8.x API.
///
/// Register as Singleton:
///   services.AddSingleton&lt;ISearchService, ElasticsearchService&gt;();
/// </summary>
public class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "nexus-content";

    public ElasticsearchService(ElasticsearchClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    private async Task EnsureIndexExistsAsync()
    {
        var exists = await _client.Indices.ExistsAsync(IndexName);
        if (exists.Exists)
            return;

        await _client.Indices.CreateAsync(IndexName, c => c
            .Mappings(m => m
                .Properties<ContentDocument>(p => p
                    .Keyword(k => k.Type)
                    .Keyword(k => k.Id)
                    .Text(t => t.Title)
                    .Text(t => t.Content!)
                    .Keyword(k => k.CreatedByUsername)
                    .Date(d => d.CreatedAt)
                    .Keyword(k => k.Tags)
                )
            )
        );
    }

    public async Task<SearchResponse> SearchAsync(
        string query,
        string? types = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var from = (page - 1) * pageSize;

        var typeFilter = types?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .ToList();

        // Build bool query
        var boolQuery = new BoolQuery
        {
            Must = new Query[]
            {
                new MultiMatchQuery
                {
                    Query = query,
                    Fields = new[] { "title^2", "content" }
                }
            }
        };

        if (typeFilter != null && typeFilter.Any())
        {
            boolQuery.Filter = new List<Query>
            {
                new TermsQuery
                {
                    Field = "type"!,
                    Terms = new TermsQueryField(typeFilter.Select(t => FieldValue.String(t)).ToArray())
                }
            };
        }

        var searchResponse = await _client.SearchAsync<ContentDocument>(s => s
            .Index(IndexName)
            .Query(boolQuery)
            .From(from)
            .Size(pageSize)
            .Highlight(h => h
                .Fields(f => f
                    .Add("title"!, hf => hf.FragmentSize(150).NumberOfFragments(1))
                    .Add("content"!, hf => hf.FragmentSize(150).NumberOfFragments(2))
                )
                .PreTags(new[] { "<em>" })
                .PostTags(new[] { "</em>" })
            )
            .Aggregations(a => a
                .Add("types", agg => agg.Terms(t => t.Field("type").Size(10)))
                .Add("tags", agg => agg.Terms(t => t.Field("tags").Size(20)))
            ),
            cancellationToken
        );

        if (!searchResponse.IsValidResponse)
        {
            var errorMessage = searchResponse.ElasticsearchServerError?.Error?.Reason ?? "Unknown error";
            throw new InvalidOperationException($"Elasticsearch search failed: {errorMessage}");
        }

        var results = new List<SearchResult>();

        if (searchResponse.Hits != null)
        {
            foreach (var hit in searchResponse.Hits)
            {
                if (hit.Source == null) continue;

                var highlights = new List<string>();
                if (hit.Highlight != null)
                {
                    foreach (var kvp in hit.Highlight)
                    {
                        highlights.AddRange(kvp.Value);
                    }
                }

                var excerpt = hit.Source.Content?.Length > 200
                    ? hit.Source.Content.Substring(0, 200) + "..."
                    : hit.Source.Content ?? "";

                results.Add(new SearchResult
                {
                    Type = hit.Source.Type,
                    Id = hit.Source.Id,
                    Title = hit.Source.Title,
                    Excerpt = excerpt,
                    Score = hit.Score ?? 0,
                    Highlights = highlights,
                    CreatedByUsername = hit.Source.CreatedByUsername,
                    CreatedAt = hit.Source.CreatedAt
                });
            }
        }

        var facets = new SearchFacets
        {
            Types = ExtractFacetCounts(searchResponse, "types"),
            Tags = ExtractFacetCounts(searchResponse, "tags")
        };

        var totalCount = searchResponse.Total > int.MaxValue ? int.MaxValue : (int)searchResponse.Total;

        return new SearchResponse
        {
            Query = query,
            Results = results,
            TotalCount = totalCount,
            Facets = facets
        };
    }

    public async Task IndexDocumentAsync(
        Guid documentId,
        string title,
        string content,
        string createdByUsername,
        DateTime createdAt,
        List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var doc = new ContentDocument
        {
            Type = "document",
            Id = documentId,
            Title = title,
            Content = content,
            CreatedByUsername = createdByUsername,
            CreatedAt = createdAt,
            Tags = tags ?? new List<string>()
        };

        await _client.IndexAsync(doc, idx => idx.Index(IndexName).Id(documentId.ToString()), cancellationToken);
    }

    public async Task IndexDiagramAsync(
        Guid diagramId,
        string title,
        string createdByUsername,
        DateTime createdAt,
        List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var doc = new ContentDocument
        {
            Type = "diagram",
            Id = diagramId,
            Title = title,
            Content = string.Empty,
            CreatedByUsername = createdByUsername,
            CreatedAt = createdAt,
            Tags = tags ?? new List<string>()
        };

        await _client.IndexAsync(doc, idx => idx.Index(IndexName).Id(diagramId.ToString()), cancellationToken);
    }

    public async Task IndexSnippetAsync(
        Guid snippetId,
        string title,
        string code,
        string language,
        string createdByUsername,
        DateTime createdAt,
        List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var doc = new ContentDocument
        {
            Type = "snippet",
            Id = snippetId,
            Title = title,
            Content = code,
            CreatedByUsername = createdByUsername,
            CreatedAt = createdAt,
            Tags = tags ?? new List<string>()
        };

        await _client.IndexAsync(doc, idx => idx.Index(IndexName).Id(snippetId.ToString()), cancellationToken);
    }

    public async Task RemoveFromIndexAsync(
        string type,
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<ContentDocument>(IndexName, resourceId.ToString(), cancellationToken);
    }

    private static Dictionary<string, int> ExtractFacetCounts(
        SearchResponse<ContentDocument> response,
        string aggregationName)
    {
        if (response.Aggregations == null || !response.Aggregations.TryGetValue(aggregationName, out var agg))
            return new Dictionary<string, int>();

        if (agg is not StringTermsAggregate termsAgg || termsAgg.Buckets == null)
            return new Dictionary<string, int>();

        var result = new Dictionary<string, int>();
        foreach (var bucket in termsAgg.Buckets)
        {
            var key = bucket.Key.ToString() ?? "";
            var count = (int)bucket.DocCount;
            result[key] = count;
        }

        return result;
    }
}

/// <summary>
/// Internal document model for Elasticsearch.
/// </summary>
internal class ContentDocument
{
    public string Type { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}
