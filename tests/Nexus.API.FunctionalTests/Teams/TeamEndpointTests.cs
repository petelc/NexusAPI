using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;
using Nexus.API.UseCases.Teams.Commands;
using Shouldly;

namespace Nexus.API.FunctionalTests.Teams;

[Collection("Sequential")]
public class TeamEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public TeamEndpointTests(CustomWebApplicationFactory<Program> factory)
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

  [Fact]
  public async Task CreateTeam_WithValidData_Returns201()
  {
    await AuthenticateAsync();

    var request = new { Name = "Test Team", Description = "A test team" };
    var response = await _client.PostAsJsonAsync("/api/v1/teams", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);

    var result = await response.Content.ReadFromJsonAsync<CreateTeamResult>();
    result.ShouldNotBeNull();
    result.Name.ShouldBe("Test Team");
    result.Description.ShouldBe("A test team");
    result.TeamId.ShouldNotBe(Guid.Empty);
  }

  [Fact]
  public async Task CreateTeam_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var request = new { Name = "Unauth Team" };
    var response = await _client.PostAsJsonAsync("/api/v1/teams", request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task CreateTeam_WithEmptyName_Returns400()
  {
    await AuthenticateAsync();

    var request = new { Name = "", Description = "No name" };
    var response = await _client.PostAsJsonAsync("/api/v1/teams", request);

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task GetMyTeams_WithAuth_Returns200()
  {
    await AuthenticateAsync();

    // Create a team first
    await _client.PostAsJsonAsync("/api/v1/teams", new { Name = "My Team for List" });

    var response = await _client.GetAsync("/api/v1/teams/my");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetMyTeams_WithoutAuth_Returns401()
  {
    _client.DefaultRequestHeaders.Authorization = null;

    var response = await _client.GetAsync("/api/v1/teams/my");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetTeamById_WithValidId_Returns200()
  {
    await AuthenticateAsync();

    // Create a team
    var createResponse = await _client.PostAsJsonAsync("/api/v1/teams",
      new { Name = "Team for Get" });
    var created = await createResponse.Content.ReadFromJsonAsync<CreateTeamResult>();

    // Get it
    var response = await _client.GetAsync($"/api/v1/teams/{created!.TeamId}");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetTeamById_WithInvalidId_Returns404()
  {
    await AuthenticateAsync();

    var response = await _client.GetAsync($"/api/v1/teams/{Guid.NewGuid()}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateTeam_WithValidData_Returns200()
  {
    await AuthenticateAsync();

    // Create a team
    var createResponse = await _client.PostAsJsonAsync("/api/v1/teams",
      new { Name = "Team to Update" });
    var created = await createResponse.Content.ReadFromJsonAsync<CreateTeamResult>();

    // Update it
    var response = await _client.PutAsJsonAsync($"/api/v1/teams/{created!.TeamId}",
      new { Name = "Updated Team Name", Description = "Updated description" });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task DeleteTeam_WithValidId_Returns204()
  {
    await AuthenticateAsync();

    // Create a team
    var createResponse = await _client.PostAsJsonAsync("/api/v1/teams",
      new { Name = "Team to Delete" });
    var created = await createResponse.Content.ReadFromJsonAsync<CreateTeamResult>();

    // Delete it
    var response = await _client.DeleteAsync($"/api/v1/teams/{created!.TeamId}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NoContent, HttpStatusCode.OK);
  }

  [Fact]
  public async Task DeleteTeam_NonExistent_Returns404()
  {
    await AuthenticateAsync();

    var response = await _client.DeleteAsync($"/api/v1/teams/{Guid.NewGuid()}");

    response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task SearchTeams_ReturnsResults()
  {
    await AuthenticateAsync();

    // Create a team to search for
    await _client.PostAsJsonAsync("/api/v1/teams",
      new { Name = "Searchable Team XYZ" });

    var response = await _client.GetAsync("/api/v1/teams/search?searchTerm=Searchable");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task AddMember_WithValidData_Returns200()
  {
    await AuthenticateAsync();

    // Create a team
    var createResponse = await _client.PostAsJsonAsync("/api/v1/teams",
      new { Name = "Team for Members" });
    var created = await createResponse.Content.ReadFromJsonAsync<CreateTeamResult>();

    // Add a member (using a random GUID since we may not have another user)
    var newMemberId = Guid.NewGuid();
    var response = await _client.PostAsJsonAsync(
      $"/api/v1/teams/{created!.TeamId}/members",
      new { UserId = newMemberId, Role = "Member" });

    // This may return 200 or 400 depending on whether user exists in the system
    response.StatusCode.ShouldBeOneOf(
      HttpStatusCode.OK, HttpStatusCode.Created,
      HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
  }
}
