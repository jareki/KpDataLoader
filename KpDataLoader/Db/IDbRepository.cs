namespace KpDataLoader.Db
{
    public interface IDbRepository<T> where T : class
    {
        Task<bool> TableExistsAsync();

        Task<bool> CreateTableAsync(string createTableQuery = null);

        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> GetWhereAsync(string whereClause, object parameters = null);

        Task<T> GetByIdAsync(object id);

        Task<object> InsertAsync(T entity);

        Task<int> InsertManyAsync(IEnumerable<T> entities, bool keepOrder = false);

        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteAsync(object id);

        Task<int> DeleteWhereAsync(string whereClause, object parameters = null);

        Task<IEnumerable<T>> QueryAsync(string sql, object parameters = null);

        Task<int> ExecuteAsync(string sql, object parameters = null);

        Task<bool> ExistsAsync(object id);

        Task<int> CountAsync();

        Task<int> CountWhereAsync(string whereClause, object parameters = null);
    }
}