namespace Nexus.API.Core.Enums;

/// <summary>
/// Represents the status of a document in its lifecycle
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document is being drafted and not yet published
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Document is published and visible to authorized users
    /// </summary>
    Published = 1,

    /// <summary>
    /// Document has been archived
    /// </summary>
    Archived = 2
}
