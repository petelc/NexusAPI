using System.Text.RegularExpressions;

namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public record Email
{
  private static readonly Regex EmailRegex = new(
    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  public string Address { get; init; }

  public Email(string address)
  {
    if (string.IsNullOrWhiteSpace(address))
      throw new ArgumentException("Email address cannot be empty", nameof(address));

    if (!EmailRegex.IsMatch(address))
      throw new ArgumentException("Invalid email address format", nameof(address));

    Address = address.ToLowerInvariant().Trim();
  }

  public override string ToString() => Address;

  public static implicit operator string(Email email) => email.Address;
}
