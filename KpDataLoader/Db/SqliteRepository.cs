using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using static Dapper.SqlMapper;

namespace KpDataLoader.Db
{
    public class SqliteRepository<T> : IDbRepository<T> where T : class
    {
        #region Fields

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _idColumnName;

        #endregion

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the SqliteRepository class
        /// </summary>
        /// <param name="dbPath">Path to the SQLite database file</param>
        /// <param name="tableName">Name of the table associated with entity T</param>
        /// <param name="idColumnName">Name of the primary key column (default: "Id")</param>
        public SqliteRepository(string dbPath, string tableName, string idColumnName = "Id")
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the database file if it doesn't exist
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            this._connectionString = $"Data Source={dbPath};Version=3;";
            this._tableName = tableName;
            this._idColumnName = idColumnName;
        }

        #endregion

        #region IDbRepository<T> Implementation

        /// <summary>
        /// Checks if the table associated with entity T exists in the database
        /// </summary>
        /// <returns>True if the table exists, false otherwise</returns>
        public async Task<bool> TableExistsAsync()
        {
            using (var connection = this.CreateConnection())
            {
                var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName";
                var result = await connection.QuerySingleOrDefaultAsync<string>(sql, new { TableName = this._tableName });
                return !string.IsNullOrEmpty(result);
            }
        }

        /// <summary>
        /// Creates the table for entity T in the database if it doesn't exist
        /// </summary>
        /// <param name="createTableQuery">Optional custom SQL for table creation</param>
        /// <returns>True if the table was created, false if it already existed</returns>
        public async Task<bool> CreateTableAsync(string createTableQuery = null)
        {
            if (await this.TableExistsAsync())
            {
                return false;
            }

            using (var connection = this.CreateConnection())
            {
                string sql;

                if (!string.IsNullOrEmpty(createTableQuery))
                {
                    sql = createTableQuery;
                }
                else
                {
                    sql = this.GenerateCreateTableQuery();
                }

                await connection.ExecuteAsync(sql);
                return true;
            }
        }

        /// <summary>
        /// Gets all entities from the table
        /// </summary>
        /// <returns>A collection of all entities</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            using (var connection = this.CreateConnection())
            {
                return await connection.QueryAsync<T>($"SELECT * FROM {this._tableName}");
            }
        }

        /// <summary>
        /// Gets entities that match the specified filter
        /// </summary>
        /// <param name="whereClause">The WHERE clause for filtering entities</param>
        /// <param name="parameters">Parameters for the where clause</param>
        /// <returns>A collection of entities matching the filter</returns>
        public async Task<IEnumerable<T>> GetWhereAsync(string whereClause, object parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                return await connection.QueryAsync<T>($"SELECT * FROM {this._tableName} WHERE {whereClause}", parameters);
            }
        }

        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <returns>The entity with the specified ID or default if not found</returns>
        public async Task<T> GetByIdAsync(object id)
        {
            using (var connection = this.CreateConnection())
            {
                var query = $"SELECT * FROM {this._tableName} WHERE {this._idColumnName} = @Id";
                return await connection.QuerySingleOrDefaultAsync<T>(query, new { Id = id });
            }
        }

        /// <summary>
        /// Inserts a new entity into the table
        /// </summary>
        /// <param name="entity">The entity to insert</param>
        /// <returns>The ID of the inserted entity</returns>
        public async Task<object> InsertAsync(T entity)
        {
            using (var connection = this.CreateConnection())
            {
                var properties = typeof(T).GetProperties()
                    .Where(p => !p.Name.Equals(this._idColumnName, StringComparison.OrdinalIgnoreCase) ||
                               !this.IsAutoIncrementId(p))
                    .ToList();

                var columnNames = string.Join(", ", properties.Select(p => p.Name));
                var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

                var sql = $"INSERT INTO {this._tableName} ({columnNames}) VALUES ({parameterNames}); SELECT last_insert_rowid();";

                var id = await connection.ExecuteScalarAsync<object>(sql, entity);
                return id;
            }
        }

        /// <summary>
        /// Inserts multiple entities into the table
        /// </summary>
        /// <param name="entities">The entities to insert</param>
        /// <param name="keepOrder">Keep order of rows while inserting to generate database-ids in right order</param>
        /// <returns>The number of affected rows</returns>
        public async Task<int> InsertManyAsync(IEnumerable<T> entities, bool keepOrder = false)
        {
            if (!entities.Any())
            {
                return 0;
            }

            using (var connection = this.CreateConnection())
            {
                var transaction = connection.BeginTransaction();
                var rowsAffected = 0;

                try
                {
                    var properties = typeof(T).GetProperties()
                        .Where(p => !p.Name.Equals(this._idColumnName, StringComparison.OrdinalIgnoreCase) ||
                                   !this.IsAutoIncrementId(p))
                        .ToList();

                    var columnNames = string.Join(", ", properties.Select(p => p.Name));
                    var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

                    var sql = $"INSERT INTO {this._tableName} ({columnNames}) VALUES ({parameterNames})";

                    if (keepOrder)
                    {
                        foreach (T entity in entities)
                        {
                            await connection.ExecuteScalarAsync<object>(sql, entity, transaction);
                            rowsAffected++;
                        }
                    }
                    else
                    {
                        rowsAffected = await connection.ExecuteAsync(sql, entities, transaction);
                    }

                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                return rowsAffected;
            }
        }

        /// <summary>
        /// Updates an existing entity in the table
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>True if the entity was updated, false otherwise</returns>
        public async Task<bool> UpdateAsync(T entity)
        {
            using (var connection = this.CreateConnection())
            {
                var properties = typeof(T).GetProperties()
                    .Where(p => !p.Name.Equals(this._idColumnName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
                var sql = $"UPDATE {this._tableName} SET {setClause} WHERE {this._idColumnName} = @{this._idColumnName}";

                var rowsAffected = await connection.ExecuteAsync(sql, entity);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Deletes an entity from the table by its ID
        /// </summary>
        /// <param name="id">The ID of the entity to delete</param>
        /// <returns>True if the entity was deleted, false otherwise</returns>
        public async Task<bool> DeleteAsync(object id)
        {
            using (var connection = this.CreateConnection())
            {
                var sql = $"DELETE FROM {this._tableName} WHERE {this._idColumnName} = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Deletes entities that match the specified filter
        /// </summary>
        /// <param name="whereClause">The WHERE clause for filtering entities to delete</param>
        /// <param name="parameters">Parameters for the where clause</param>
        /// <returns>The number of deleted entities</returns>
        public async Task<int> DeleteWhereAsync(string whereClause, object parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                var sql = $"DELETE FROM {this._tableName} WHERE {whereClause}";
                return await connection.ExecuteAsync(sql, parameters);
            }
        }

        /// <summary>
        /// Executes a custom query with parameters
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <returns>A collection of entities matching the query</returns>
        public async Task<IEnumerable<T>> QueryAsync(string sql, object parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                return await connection.QueryAsync<T>(sql, parameters);
            }
        }

        /// <summary>
        /// Executes a custom non-query command with parameters
        /// </summary>
        /// <param name="sql">The SQL command to execute</param>
        /// <param name="parameters">The parameters for the command</param>
        /// <returns>The number of affected rows</returns>
        public async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                return await connection.ExecuteAsync(sql, parameters);
            }
        }

        /// <summary>
        /// Checks if an entity with the specified ID exists
        /// </summary>
        /// <param name="id">The ID to check</param>
        /// <returns>True if an entity with the specified ID exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(object id)
        {
            using (var connection = this.CreateConnection())
            {
                var sql = $"SELECT COUNT(1) FROM {this._tableName} WHERE {this._idColumnName} = @Id";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
                return count > 0;
            }
        }

        /// <summary>
        /// Counts the number of entities in the table
        /// </summary>
        /// <returns>The number of entities</returns>
        public async Task<int> CountAsync()
        {
            using (var connection = this.CreateConnection())
            {
                var sql = $"SELECT COUNT(1) FROM {this._tableName}";
                return await connection.ExecuteScalarAsync<int>(sql);
            }
        }

        /// <summary>
        /// Counts the number of entities that match the specified filter
        /// </summary>
        /// <param name="whereClause">The WHERE clause for filtering entities</param>
        /// <param name="parameters">Parameters for the where clause</param>
        /// <returns>The number of entities matching the filter</returns>
        public async Task<int> CountWhereAsync(string whereClause, object parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                var sql = $"SELECT COUNT(1) FROM {this._tableName} WHERE {whereClause}";
                return await connection.ExecuteScalarAsync<int>(sql, parameters);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new database connection
        /// </summary>
        /// <returns>An open SQLite connection</returns>
        private IDbConnection CreateConnection()
        {
            var connection = new SQLiteConnection(this._connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Generates a CREATE TABLE query based on entity T's properties
        /// </summary>
        /// <returns>SQL query for table creation</returns>
        private string GenerateCreateTableQuery()
        {
            var properties = typeof(T).GetProperties();
            var columns = new List<string>();

            foreach (var prop in properties)
            {
                var columnName = prop.Name;
                var columnType = this.GetSqliteType(prop.PropertyType);
                var constraints = "";

                // Add PRIMARY KEY constraint to ID column
                if (columnName.Equals(this._idColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    constraints = "PRIMARY KEY AUTOINCREMENT";
                }

                columns.Add($"{columnName} {columnType} {constraints}".Trim());
            }

            return $"CREATE TABLE {this._tableName} ({string.Join(", ", columns)})";
        }

        /// <summary>
        /// Maps .NET types to SQLite data types
        /// </summary>
        /// <param name="type">The .NET type to map</param>
        /// <returns>The corresponding SQLite type</returns>
        private string GetSqliteType(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                type = nullableType;
            }

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
                type == typeof(byte) || type == typeof(sbyte) || type.IsEnum)
            {
                return "INTEGER";
            }
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            {
                return "REAL";
            }
            if (type == typeof(bool))
            {
                return "INTEGER"; // SQLite has no boolean, use INTEGER instead
            }
            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                return "TEXT"; // Store as ISO8601 strings
            }
            if (type == typeof(Guid))
            {
                return "TEXT"; // Store as string
            }
            if (type == typeof(byte[]))
            {
                return "BLOB";
            }
            return "TEXT"; // Default for string and other types
        }

        /// <summary>
        /// Determines if a property is an auto-incrementing ID
        /// </summary>
        /// <param name="property">The property to check</param>
        /// <returns>True if the property is an auto-incrementing ID, false otherwise</returns>
        private bool IsAutoIncrementId(PropertyInfo property)
        {
            // Check if it's the ID column and an integer type
            if (property.Name.Equals(this._idColumnName, StringComparison.OrdinalIgnoreCase))
            {
                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                return type == typeof(int) || type == typeof(long);
            }
            return false;
        }

        #endregion
    }
}
