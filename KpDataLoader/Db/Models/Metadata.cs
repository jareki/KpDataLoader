namespace KpDataLoader.Db.Models;

/// <summary>
/// Represents metadata about the database
/// </summary>
public class Metadata
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Date when the database was generated
    /// </summary>
    public string LastUpdate { get; set; }

    /// <summary>
    /// Total count of movies in the database
    /// </summary>
    public int MovieCount { get; set; }
}