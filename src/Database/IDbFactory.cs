using MongoDB.Driver;

namespace ITLab.Salary.Database
{
    /// <summary>
    /// Create and returns database for concrete class
    /// </summary>
    public interface IDbFactory
    {
        /// <summary>
        /// Get database fot class <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Created database</returns>
        IMongoDatabase GetDatabase<T>();
    }
}