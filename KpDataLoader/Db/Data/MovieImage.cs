namespace KpDataLoader.Db.Data;

/// <summary>
/// Represents an image for a movie
/// </summary>
public class MovieImage
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the movie in Movies table
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// URL to the image on KinoPoisk
    /// </summary>
    public string Uri { get; set; }
}