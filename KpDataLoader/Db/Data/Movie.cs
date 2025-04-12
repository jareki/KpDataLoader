namespace KpDataLoader.Db.Data;

/// <summary>
/// Represents a movie entry in the database
/// </summary>
public class Movie
{
    /// <summary>
    /// Unique identifier for the movie
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Russian name of the movie
    /// </summary>
    public string NameRu { get; set; }

    /// <summary>
    /// English name of the movie
    /// </summary>
    public string NameEn { get; set; }

    /// <summary>
    /// External ID in KinoPoisk
    /// </summary>
    public int KpId { get; set; }

    /// <summary>
    /// Release year of the movie
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Reference to the movie type in Types table
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// KinoPoisk rating
    /// </summary>
    public double RatingKp { get; set; }

    /// <summary>
    /// IMDb rating
    /// </summary>
    public double RatingImdb { get; set; }

    /// <summary>
    /// Number of votes on KinoPoisk
    /// </summary>
    public int VotesKp { get; set; }

    /// <summary>
    /// Number of votes on IMDb
    /// </summary>
    public int VotesImdb { get; set; }

    /// <summary>
    /// Date and time of the last update
    /// </summary>
    public string LastUpdate { get; set; }

    /// <summary>
    /// Date and time of the last images update
    /// </summary>
    public string LastImagesUpdate { get; set; }
}