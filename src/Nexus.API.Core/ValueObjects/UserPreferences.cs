namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Theme preference
/// </summary>
public enum Theme
{
  Light = 0,
  Dark = 1,
  Auto = 2
}

/// <summary>
/// Email digest frequency
/// </summary>
public enum EmailDigest
{
  None = 0,
  Daily = 1,
  Weekly = 2
}

/// <summary>
/// User preferences value object
/// </summary>
public record UserPreferences
{
  public Theme Theme { get; init; }
  public string Language { get; init; }
  public bool NotificationsEnabled { get; init; }
  public EmailDigest EmailDigest { get; init; }

  public UserPreferences(
    Theme theme = Theme.Auto,
    string language = "en-US",
    bool notificationsEnabled = true,
    EmailDigest emailDigest = EmailDigest.Weekly)
  {
    Theme = theme;
    Language = language ?? "en-US";
    NotificationsEnabled = notificationsEnabled;
    EmailDigest = emailDigest;
  }

  public static UserPreferences Default => new();

  public UserPreferences UpdateTheme(Theme theme) =>
    this with { Theme = theme };

  public UserPreferences UpdateLanguage(string language) =>
    this with { Language = language };

  public UserPreferences UpdateNotifications(bool enabled) =>
    this with { NotificationsEnabled = enabled };

  public UserPreferences UpdateEmailDigest(EmailDigest digest) =>
    this with { EmailDigest = digest };
}
