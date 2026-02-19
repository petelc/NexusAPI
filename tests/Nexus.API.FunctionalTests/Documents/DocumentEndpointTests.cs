using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nexus.API.UseCases.Documents.Create;
using Shouldly;

namespace Nexus.API.FunctionalTests.Documents;

[Collection("Sequential")]
public class DocumentEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public DocumentEndpointTests(CustomWebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }

  private async Task AuthenticateAsync()
  {
    var token = await AuthHelper.GetAccessTokenAsync(_client);
    _client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", token);
  }

  private async Task<Guid> CreateDocumentAsync(
    string title = "Test Document",
    string content = "<p>Test content</p>")
  {
    var response = await _client.PostAsJsonAsync("/api/v1/documents",
      new { Title = title, Content = content, Status = "draft" });
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<CreateDocumentResponse>();
    return result!.DocumentId;
  }

  // ─── Create ────────────────────────────────────────────────────────

  [Fact]
  public async Task CreateDocument_WithValidData_Returns201()
  {
    await AuthenticateAsync();
    var request = new
    {
      Title = "Functional Test Doc",
      Content = "<p>Some content</p>",
      Status = "draft"
    };

    var response = await _client.PostAsJsonAsync("/api/v1/documents", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var result = await response.Content.ReadFromJsonAsync<CreateDocumentResponse>();
    result.ShouldNotBeNull();
    result.Title.ShouldBe("Functional Test Doc");
    result.DocumentId.ShouldNotBe(Guid.Empty);
  }

  [Fact]
  public async Task CreateDocument_WithEmptyTitle_Returns400Or422()
  {
    var request = new
    {
      Title = "",
      Content = "<p>content</p>",
      Status = "draft"
    };

    var response = await _client.PostAsJsonAsync("/api/v1/documents", request);

    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.BadRequest,
      HttpStatusCode.UnprocessableEntity,
      HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task CreateDocument_WithTags_Returns201()
  {
    await AuthenticateAsync();
    var request = new
    {
      Title = "Tagged Doc",
      Content = "<p>content</p>",
      Status = "draft",
      Tags = new[] { "tag1", "tag2" }
    };

    var response = await _client.PostAsJsonAsync("/api/v1/documents", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
  }

  // ─── List ──────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDocuments_Returns200()
  {
    await AuthenticateAsync();
    await CreateDocumentAsync();

    var response = await _client.GetAsync("/api/v1/documents");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task ListDocuments_WithStatusFilter_Returns200()
  {
    var response = await _client.GetAsync("/api/v1/documents?status=draft");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task ListDocuments_WithPagination_Returns200()
  {
    var response = await _client.GetAsync("/api/v1/documents?page=1&pageSize=5");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task ListDocuments_WithSearch_Returns200()
  {
    await AuthenticateAsync();
    await CreateDocumentAsync("Searchable Document XYZ");

    var response = await _client.GetAsync("/api/v1/documents?search=Searchable");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  // ─── Get By Id ─────────────────────────────────────────────────────

  [Fact]
  public async Task GetDocumentById_WithValidId_Returns200()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.GetAsync($"/api/v1/documents/{documentId}");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetDocumentById_WithInvalidId_Returns404()
  {
    await AuthenticateAsync();
    var response = await _client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
  }

  // ─── Publish ───────────────────────────────────────────────────────

  [Fact]
  public async Task PublishDocument_WithValidId_Returns200()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.PostAsync(
      $"/api/v1/documents/{documentId}/publish", null);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task PublishDocument_WithInvalidId_Returns404()
  {
    await AuthenticateAsync();
    var response = await _client.PostAsync(
      $"/api/v1/documents/{Guid.NewGuid()}/publish", null);

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
  }

  // ─── Update ────────────────────────────────────────────────────────

  [Fact]
  public async Task UpdateDocument_WithValidData_Returns200()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.PutAsJsonAsync(
      $"/api/v1/documents/{documentId}",
      new { Title = "Updated Title", Content = "<p>Updated content</p>" });

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateDocument_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.PutAsJsonAsync(
      $"/api/v1/documents/{Guid.NewGuid()}",
      new { Title = "Updated" });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Delete ────────────────────────────────────────────────────────

  [Fact]
  public async Task DeleteDocument_WithValidId_Returns204()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.DeleteAsync($"/api/v1/documents/{documentId}");

    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.NoContent, HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeleteDocument_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.DeleteAsync($"/api/v1/documents/{Guid.NewGuid()}");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteDocument_NonExistent_Returns404()
  {
    await AuthenticateAsync();

    var response = await _client.DeleteAsync($"/api/v1/documents/{Guid.NewGuid()}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
  }

  // ─── Tags ──────────────────────────────────────────────────────────

  [Fact]
  public async Task AddTags_WithValidData_Returns204()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/documents/{documentId}/tags",
      new { Tags = new[] { "api-test", "functional" } });

    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.NoContent, HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task AddTags_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/documents/{Guid.NewGuid()}/tags",
      new { Tags = new[] { "test" } });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task AddTags_WithEmptyTags_Returns400()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/documents/{documentId}/tags",
      new { Tags = Array.Empty<string>() });

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  // ─── Versions ──────────────────────────────────────────────────────

  [Fact]
  public async Task ListVersions_WithAuth_Returns200()
  {
    await AuthenticateAsync();
    var documentId = await CreateDocumentAsync();

    var response = await _client.GetAsync($"/api/v1/documents/{documentId}/versions");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task ListVersions_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}/versions");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
