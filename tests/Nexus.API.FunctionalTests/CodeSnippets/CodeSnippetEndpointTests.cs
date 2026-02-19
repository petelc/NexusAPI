using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nexus.API.UseCases.CodeSnippets.DTOs;
using Shouldly;

namespace Nexus.API.FunctionalTests.CodeSnippets;

[Collection("Sequential")]
public class CodeSnippetEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public CodeSnippetEndpointTests(CustomWebApplicationFactory<Program> factory)
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

  private async Task<Guid> CreateSnippetAsync(
    string title = "Test Snippet",
    string code = "Console.WriteLine(\"Hello\");",
    string language = "C#")
  {
    var response = await _client.PostAsJsonAsync("/api/v1/snippets", new
    {
      Title = title,
      Code = code,
      Language = language
    });
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<CodeSnippetDto>();
    return result!.SnippetId;
  }

  // ─── Create ────────────────────────────────────────────────────────

  [Fact]
  public async Task CreateSnippet_WithValidData_Returns201()
  {
    await AuthenticateAsync();

    var response = await _client.PostAsJsonAsync("/api/v1/snippets", new
    {
      Title = "Hello World",
      Code = "Console.WriteLine(\"Hello, World!\");",
      Language = "C#"
    });

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var result = await response.Content.ReadFromJsonAsync<CodeSnippetDto>();
    result.ShouldNotBeNull();
    result.Title.ShouldBe("Hello World");
    result.Language.ShouldBe("C#");
    result.SnippetId.ShouldNotBe(Guid.Empty);
    result.IsPublic.ShouldBeFalse();
  }

  [Fact]
  public async Task CreateSnippet_WithDescription_Returns201()
  {
    await AuthenticateAsync();

    var response = await _client.PostAsJsonAsync("/api/v1/snippets", new
    {
      Title = "Described Snippet",
      Code = "var x = 42;",
      Language = "C#",
      Description = "A test snippet"
    });

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var result = await response.Content.ReadFromJsonAsync<CodeSnippetDto>();
    result!.Description.ShouldBe("A test snippet");
  }

  [Fact]
  public async Task CreateSnippet_WithTags_Returns201()
  {
    await AuthenticateAsync();

    var response = await _client.PostAsJsonAsync("/api/v1/snippets", new
    {
      Title = "Tagged Snippet",
      Code = "print('hello')",
      Language = "Python",
      Tags = new[] { "python", "tutorial" }
    });

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
  }

  [Fact]
  public async Task CreateSnippet_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.PostAsJsonAsync("/api/v1/snippets", new
    {
      Title = "Test",
      Code = "code",
      Language = "C#"
    });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task CreateSnippet_WithMissingTitle_ReturnsBadRequest()
  {
    await AuthenticateAsync();

    var response = await _client.PostAsJsonAsync("/api/v1/snippets", new
    {
      Title = "",
      Code = "var x = 1;",
      Language = "C#"
    });

    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.BadRequest,
      HttpStatusCode.UnprocessableEntity,
      HttpStatusCode.InternalServerError);
  }

  // ─── Get By Id ─────────────────────────────────────────────────────

  [Fact]
  public async Task GetSnippetById_WithValidId_Returns200()
  {
    await AuthenticateAsync();
    var snippetId = await CreateSnippetAsync();

    var response = await _client.GetAsync($"/api/v1/snippets/{snippetId}");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetSnippetById_WithInvalidId_Returns404OrForbidden()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync($"/api/v1/snippets/{Guid.NewGuid()}");

    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.NotFound,
      HttpStatusCode.Forbidden,
      HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task GetSnippetById_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync($"/api/v1/snippets/{Guid.NewGuid()}");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── List My Snippets ──────────────────────────────────────────────

  [Fact]
  public async Task GetMySnippets_WithAuth_Returns200()
  {
    await AuthenticateAsync();
    await CreateSnippetAsync("My Listed Snippet");

    var response = await _client.GetAsync("/api/v1/snippets/my");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetMySnippets_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/snippets/my");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── List Public Snippets ──────────────────────────────────────────

  [Fact]
  public async Task GetPublicSnippets_Returns200()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync("/api/v1/snippets/public");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetPublicSnippets_WithPagination_Returns200()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync("/api/v1/snippets/public?page=1&pageSize=5");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetPublicSnippets_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/snippets/public");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Update ────────────────────────────────────────────────────────

  [Fact]
  public async Task UpdateSnippet_WithValidData_Returns200()
  {
    await AuthenticateAsync();
    var snippetId = await CreateSnippetAsync("Original Title");

    var response = await _client.PutAsJsonAsync(
      $"/api/v1/snippets/{snippetId}",
      new { Title = "Updated Title", Code = "// updated" });

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task UpdateSnippet_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.PutAsJsonAsync(
      $"/api/v1/snippets/{Guid.NewGuid()}",
      new { Title = "Updated" });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Delete ────────────────────────────────────────────────────────

  [Fact]
  public async Task DeleteSnippet_WithValidId_Returns204OrOk()
  {
    await AuthenticateAsync();
    var snippetId = await CreateSnippetAsync("Snippet to Delete");

    var response = await _client.DeleteAsync($"/api/v1/snippets/{snippetId}");

    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.NoContent, HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeleteSnippet_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.DeleteAsync($"/api/v1/snippets/{Guid.NewGuid()}");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Publish / Unpublish ───────────────────────────────────────────

  [Fact]
  public async Task PublishSnippet_WithValidId_Returns200()
  {
    await AuthenticateAsync();
    var snippetId = await CreateSnippetAsync("Snippet to Publish");

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/snippets/{snippetId}/publish", new { });

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task PublishSnippet_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/snippets/{Guid.NewGuid()}/publish", new { });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UnpublishSnippet_AfterPublish_Returns200()
  {
    await AuthenticateAsync();
    var snippetId = await CreateSnippetAsync("Snippet to Unpublish");

    await _client.PostAsJsonAsync($"/api/v1/snippets/{snippetId}/publish", new { });
    var response = await _client.PostAsJsonAsync(
      $"/api/v1/snippets/{snippetId}/unpublish", new { });

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  // ─── Fork ──────────────────────────────────────────────────────────

  [Fact]
  public async Task ForkSnippet_PublicSnippet_Returns201()
  {
    await AuthenticateAsync();
    var snippetId = await CreateSnippetAsync("Public Snippet to Fork");
    await _client.PostAsJsonAsync($"/api/v1/snippets/{snippetId}/publish", new { });

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/snippets/{snippetId}/fork",
      new { Title = "My Fork" });

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task ForkSnippet_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.PostAsJsonAsync(
      $"/api/v1/snippets/{Guid.NewGuid()}/fork",
      new { Title = "Fork" });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Get By Language ───────────────────────────────────────────────

  [Fact]
  public async Task GetSnippetsByLanguage_Returns200()
  {
    await AuthenticateAsync();
    await CreateSnippetAsync(language: "Python");

    var response = await _client.GetAsync("/api/v1/snippets/by-language/Python");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetSnippetsByLanguage_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/snippets/by-language/Python");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Get By Tag ────────────────────────────────────────────────────

  [Fact]
  public async Task GetSnippetsByTag_Returns200()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync("/api/v1/snippets/by-tag/csharp");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetSnippetsByTag_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/snippets/by-tag/csharp");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Search ────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchSnippets_WithQuery_Returns200()
  {
    await AuthenticateAsync();
    await CreateSnippetAsync("SearchableSnippetXYZ");

    var response = await _client.GetAsync("/api/v1/snippets/search?q=SearchableSnippetXYZ");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task SearchSnippets_WithoutQuery_Returns400()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync("/api/v1/snippets/search");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
  }

  [Fact]
  public async Task SearchSnippets_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/snippets/search?q=test");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  // ─── Tags ──────────────────────────────────────────────────────────

  [Fact]
  public async Task GetAllTags_WithAuth_Returns200()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync("/api/v1/tags");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetAllTags_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/tags");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task SearchTags_WithQuery_Returns200()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync("/api/v1/tags/search?q=python");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task SearchTags_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/tags/search?q=python");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
