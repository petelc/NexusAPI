namespace Nexus.API.Core.ValueObjects;

/// <summary>
/// Person name value object
/// </summary>
public record PersonName
{
  public string FirstName { get; init; }
  public string LastName { get; init; }

  public PersonName(string firstName, string lastName)
  {
    if (string.IsNullOrWhiteSpace(firstName))
      throw new ArgumentException("First name cannot be empty", nameof(firstName));

    if (string.IsNullOrWhiteSpace(lastName))
      throw new ArgumentException("Last name cannot be empty", nameof(lastName));

    FirstName = firstName.Trim();
    LastName = lastName.Trim();
  }

  public string FullName => $"{FirstName} {LastName}";

  public override string ToString() => FullName;
}
