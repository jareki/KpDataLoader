using KpDataLoader.Db.Models;
using KpDataLoader.Helpers;

namespace KpDataLoader.Db
{
    /// <summary>
    /// Service for managing movie database operations
    /// </summary>
    public class DataService : IDisposable
    {
        private const string MovieTableName = "Movies";
        private const string MetadataTableName = "Metadata";
        private const string MovieTypeTableName = "MovieTypes";
        private const string MovieImageTableName = "MovieImages";

        private readonly IDbRepository<Movie> _movieRepository;
        private readonly IDbRepository<Metadata> _metadataRepository;
        private readonly IDbRepository<MovieType> _movieTypeRepository;
        private readonly IDbRepository<MovieImage> _movieImageRepository;

        /// <summary>
        /// Initializes a new instance of the MovieDataService
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public DataService(string databasePath)
        {
            this._movieRepository = new SqliteRepository<Movie>(databasePath, MovieTableName);
            this._metadataRepository = new SqliteRepository<Metadata>(databasePath, MetadataTableName);
            this._movieTypeRepository = new SqliteRepository<MovieType>(databasePath, MovieTypeTableName);
            this._movieImageRepository = new SqliteRepository<MovieImage>(databasePath, MovieImageTableName);
        }

        /// <summary>
        /// Initializes the database by creating tables if they don't exist
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            if (!await this._movieRepository.TableExistsAsync())
            {
                await this._movieRepository.CreateTableAsync();
            }

            if (!await this._metadataRepository.TableExistsAsync())
            {
                await this._metadataRepository.CreateTableAsync();

                // Initialize with default metadata
                await this._metadataRepository.InsertAsync(new Metadata
                {
                    LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat),
                    MovieCount = 0
                });
            }

            if (!await this._movieTypeRepository.TableExistsAsync())
            {
                await this._movieTypeRepository.CreateTableAsync();
            }
            await this._movieTypeRepository.InsertManyAsync(MovieType.Instances, true);

            if (!await this._movieImageRepository.TableExistsAsync())
            {
                await this._movieImageRepository.CreateTableAsync();
            }
        }

        /// <summary>
        /// Adds a new movie to the database
        /// </summary>
        /// <param name="movie">The movie to add</param>
        /// <returns>The ID of the added movie</returns>
        public async Task<int> AddMovieAsync(Movie movie)
        {
            movie.LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat);
            movie.LastImagesUpdate = DateTime.MinValue.ToString(DateHelper.SqliteFormat);
            var result = await this._movieRepository.InsertAsync(movie);

            await this.UpdateMovieCountAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Updates an existing movie in the database
        /// </summary>
        /// <param name="movie">The movie to update</param>
        /// <returns>True if the update was successful, false otherwise</returns>
        public async Task<bool> UpdateMovieAsync(Movie movie)
        {
            if (!await this._movieRepository.ExistsAsync(movie.Id))
            {
                return false;
            }
            movie.LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat);
            return await this._movieRepository.UpdateAsync(movie);
        }

        /// <summary>
        /// Adds a new image for a movie
        /// </summary>
        /// <param name="movieImage">The image to add</param>
        /// <returns>The ID of the added image</returns>
        public async Task<int> AddMovieImageAsync(MovieImage movieImage)
        {
            if (!await this._movieRepository.ExistsAsync(movieImage.MovieId))
            {
                throw new InvalidOperationException($"Movie with ID {movieImage.MovieId} does not exist");
            }

            var result = await this._movieImageRepository.InsertAsync(movieImage);
            await this._movieRepository.ExecuteAsync(
                "UPDATE Movies SET LastImagesUpdate = @LastUpdate WHERE Id = @Id",
                new { LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat), Id = movieImage.MovieId });

            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Adds multiple images for a movie
        /// </summary>
        /// <param name="movieId">The ID of the movie</param>
        /// <param name="imageUris">The list of image URIs to add</param>
        /// <returns>The number of images added</returns>
        public async Task<int> AddMovieImagesAsync(int movieId, IEnumerable<string> imageUris)
        {
            if (!await this._movieRepository.ExistsAsync(movieId))
            {
                throw new InvalidOperationException($"Movie with ID {movieId} does not exist");
            }

            var movieImages = imageUris
                .Select(uri => new MovieImage { MovieId = movieId, Uri = uri })
                .ToList();

            var rowsAffected = await this._movieImageRepository.InsertManyAsync(movieImages);

            await this._movieRepository.ExecuteAsync(
                "UPDATE Movies SET LastImagesUpdate = @LastUpdate WHERE Id = @Id",
                new { LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat), Id = movieId });

            return rowsAffected;
        }

        /// <summary>
        /// Deletes all images for a movie
        /// </summary>
        /// <param name="movieId">The ID of the movie</param>
        /// <returns>The number of images deleted</returns>
        public async Task<int> DeleteMovieImagesAsync(int movieId)
        {
            if (!await this._movieRepository.ExistsAsync(movieId))
            {
                throw new InvalidOperationException($"Movie with ID {movieId} does not exist");
            }

            return await this._movieImageRepository.DeleteWhereAsync("MovieId = @MovieId", new { MovieId = movieId });
        }

        /// <summary>
        /// Updates the metadata of the database
        /// </summary>
        /// <returns>True if the update was successful, false otherwise</returns>
        public async Task<bool> UpdateMetadataAsync()
        {
            var metadata = (await this._metadataRepository.GetAllAsync()).FirstOrDefault();

            if (metadata == null)
            {
                await this._metadataRepository.InsertAsync(new Metadata
                {
                    LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat),
                    MovieCount = await this._movieRepository.CountAsync()
                });
                return true;
            }
            else
            {
                metadata.LastUpdate = DateTime.Now.ToString(DateHelper.SqliteFormat);
                metadata.MovieCount = await this._movieRepository.CountAsync();
                return await this._metadataRepository.UpdateAsync(metadata);
            }
        }

        /// <summary>
        /// Updates the movie count in the metadata
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task UpdateMovieCountAsync()
        {
            var metadata = (await this._metadataRepository.GetAllAsync()).FirstOrDefault();

            if (metadata != null)
            {
                metadata.MovieCount = await this._movieRepository.CountAsync();
                await this._metadataRepository.UpdateAsync(metadata);
            }
        }

        /// <summary>
        /// Найти фильм по айди кинопоиска
        /// </summary>
        /// <param name="id">айди фильма в кп</param>
        /// <returns>The movie if found, null otherwise</returns>
        public async Task<Movie?> GetMovieByIdAsync(int kpId)
        {
            return (await this._movieRepository.GetWhereAsync("KpId = @KpId ", new { KpId = kpId }))?.FirstOrDefault();
        }

        public async Task<Movie> GetOldestUpdatedMovie()
        {
            return (await this._movieRepository.GetWhereAsync("MovieId is not null order by LastUpdate desc limit 1")).FirstOrDefault();
        }

        public async Task<Movie> GetOldestImagesUpdatedMovie()
        {
            return (await this._movieRepository.GetWhereAsync("MovieId is not null order by LastImagesUpdate desc limit 1")).FirstOrDefault();
        }

        /// <summary>
        /// Gets all movies from the database
        /// </summary>
        /// <returns>A collection of all movies</returns>
        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await this._movieRepository.GetAllAsync();
        }

        /// <summary>
        /// Gets all images for a movie
        /// </summary>
        /// <param name="movieId">The ID of the movie</param>
        /// <returns>A collection of images for the movie</returns>
        public async Task<IEnumerable<MovieImage>> GetMovieImagesAsync(int movieId)
        {
            return await this._movieImageRepository.GetWhereAsync("MovieId = @MovieId", new { MovieId = movieId });
        }

        /// <summary>
        /// Gets the current metadata
        /// </summary>
        /// <returns>The current metadata</returns>
        public async Task<Metadata> GetMetadataAsync()
        {
            return (await this._metadataRepository.GetAllAsync()).FirstOrDefault();
        }

        /// <summary>
        /// Disposes of resources
        /// </summary>
        public void Dispose()
        {
            // No specific disposal needed for SQLite repositories
            // as they create and dispose connections as needed
        }
    }
}