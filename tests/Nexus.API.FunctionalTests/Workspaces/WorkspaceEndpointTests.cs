using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nexus.API.UseCases.Teams.Commands;
using Shouldly;

namespace Nexus.API.FunctionalTests.Workspaces;

[Collection("Sequential")]
public class WorkspaceEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public WorkspaceEndpointTests(CustomWebApplicationFactory<Program> factory)
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

  private async Task<Guid> CreateTeamAsync(string name = "")
  {
    var teamName = string.IsNullOrEmpty(name)
      ? $"Workspace-Test-Team-{Guid.NewGuid():N}"
      : name;
    var response = await _client.PostAsJsonAsync("/api/v1/teams", new { Name = teamName });
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<CreateTeamResult>();
    return result!.TeamId;
  }

  [Fact]
  public async Task CreateWorkspace_WithValidData_Returns201()
  {
    await AuthenticateAsync();
    var teamId = await CreateTeamAsync();

    var request = new
    {
      Name = "Test Workspace",
      Description = "A test workspace",
      TeamId = teamId
    };

    var response = await _client.PostAsJsonAsync("/api/v1/workspaces", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var content = await response.Content.ReadAsStringAsync();
    content.ShouldContain("Test Workspace");
  }

  [Fact]
  public async Task CreateWorkspace_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var request = new { Name = "Unauth Workspace", TeamId = Guid.NewGuid() };
    var response = await _client.PostAsJsonAsync("/api/v1/workspaces", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task CreateWorkspace_WithEmptyName_Returns400()
  {
    await AuthenticateAsync();
    var teamId = await CreateTeamAsync();

    var request = new { Name = "", TeamId = teamId };
    var response = await _client.PostAsJsonAsync("/api/v1/workspaces", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateWorkspace_WithEmptyTeamId_Returns400()
  {
    await AuthenticateAsync();

    var request = new { Name = "No Team Workspace", TeamId = Guid.Empty };
    var response = await _client.PostAsJsonAsync("/api/v1/workspaces", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetMyWorkspaces_WithAuth_Returns200()
  {
    await AuthenticateAsync();

    // Create a team and workspace first
    var teamId = await CreateTeamAsync();
    await _client.PostAsJsonAsync("/api/v1/workspaces",
      new { Name = "My Workspace for List", TeamId = teamId });

    var response = await _client.GetAsync("/api/v1/workspaces/my");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetMyWorkspaces_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/workspaces/my");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetWorkspaceById_WithValidId_Returns200()
  {
    await AuthenticateAsync();

    // Create team and workspace
    var teamId = await CreateTeamAsync();
    var createResponse = await _client.PostAsJsonAsync("/api/v1/workspaces",
      new { Name = "Workspace for Get", TeamId = teamId });
    var content = await createResponse.Content.ReadAsStringAsync();

    // Extract workspaceId from response
    var doc = System.Text.Json.JsonDocument.Parse(content);
    var workspaceId = doc.RootElement.GetProperty("workspaceId").GetGuid();

    // Get it
    var response = await _client.GetAsync($"/api/v1/workspaces/{workspaceId}");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetWorkspaceById_WithInvalidId_Returns404()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync($"/api/v1/workspaces/{Guid.NewGuid()}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task GetTeamWorkspaces_Returns200()
  {
    await AuthenticateAsync();

    // Create team and workspace
    var teamId = await CreateTeamAsync("Team with Workspaces");
    await _client.PostAsJsonAsync("/api/v1/workspaces",
      new { Name = "Team Workspace", TeamId = teamId });

    var response = await _client.GetAsync($"/api/v1/teams/{teamId}/workspaces");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task UpdateWorkspace_WithValidData_Returns200()
  {
    await AuthenticateAsync();

    // Create team and workspace
    var teamId = await CreateTeamAsync();
    var createResponse = await _client.PostAsJsonAsync("/api/v1/workspaces",
      new { Name = "Workspace to Update", TeamId = teamId });
    var content = await createResponse.Content.ReadAsStringAsync();
    var doc = System.Text.Json.JsonDocument.Parse(content);
    var workspaceId = doc.RootElement.GetProperty("workspaceId").GetGuid();

    // Update
    var response = await _client.PutAsJsonAsync($"/api/v1/workspaces/{workspaceId}",
      new { Name = "Updated Workspace", Description = "Updated" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task DeleteWorkspace_WithValidId_Returns204()
  {
    await AuthenticateAsync();

    // Create team and workspace
    var teamId = await CreateTeamAsync();
    var createResponse = await _client.PostAsJsonAsync("/api/v1/workspaces",
      new { Name = "Workspace to Delete", TeamId = teamId });
    var content = await createResponse.Content.ReadAsStringAsync();
    var doc = System.Text.Json.JsonDocument.Parse(content);
    var workspaceId = doc.RootElement.GetProperty("workspaceId").GetGuid();

    // Delete
    var response = await _client.DeleteAsync($"/api/v1/workspaces/{workspaceId}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
  }

  [Fact]
  public async Task SearchWorkspaces_ReturnsResults()
  {
    await AuthenticateAsync();

    // Create team and workspace
    var teamId = await CreateTeamAsync();
    await _client.PostAsJsonAsync("/api/v1/workspaces",
      new { Name = "Searchable Workspace ABC", TeamId = teamId });

    var response = await _client.GetAsync("/api/v1/workspaces/search?searchTerm=Searchable");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }
}
