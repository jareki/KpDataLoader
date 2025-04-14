namespace KpDataLoader.Db.Models;

/// <summary>
/// Represents a movie type
/// </summary>
public class MovieType
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    public static List<MovieType> Instances = new List<MovieType>();

    public MovieType()
    {
    }

    public MovieType(int id, string name)
    {
        this.Id = id;
        this.Name = name;

        Instances.Add(this);
    }

    public static MovieType Movie = new MovieType(1, "movie");

    public static MovieType TvSeries = new MovieType(2, "tv-series");

    public static MovieType Cartoon = new MovieType(3, "cartoon");

    public static MovieType Anime = new MovieType(4, "anime");

    public static MovieType AnimatedSeries = new MovieType(5, "animated-series");

    
}