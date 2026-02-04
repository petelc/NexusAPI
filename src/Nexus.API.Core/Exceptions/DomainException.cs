namespace Nexus.API.Core.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated
/// Used throughout aggregates to enforce business rules
/// </summary>
public class DomainException : Exception
{
  public DomainException(string message) : base(message)
  {
  }

  public DomainException(string message, Exception innerException) 
    : base(message, innerException)
  {
  }
}
