namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// User profile value object
/// </summary>
public record UserProfile
{
  public string? AvatarUrl { get; init; }
  public string? Bio { get; init; }
  public string? Title { get; init; }
  public string? Department { get; init; }

  public UserProfile(
    string? avatarUrl = null,
    string? bio = null,
    string? title = null,
    string? department = null)
  {
    AvatarUrl = avatarUrl;
    Bio = bio;
    Title = title;
    Department = department;
  }

  public static UserProfile Empty => new();

  public UserProfile UpdateAvatar(string avatarUrl) =>
    this with { AvatarUrl = avatarUrl };

  public UserProfile UpdateBio(string bio) =>
    this with { Bio = bio };

  public UserProfile UpdateTitle(string title) =>
    this with { Title = title };

  public UserProfile UpdateDepartment(string department) =>
    this with { Department = department };
}
