using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.MongoMigrations
{
    /// <summary>
    /// Information about migration
    /// </summary>
    public abstract class MongoMigration
    {
        /// <summary>
        /// Unique migration Id
        /// </summary>
        public abstract Guid Id { get; }
        /// <summary>
        /// Date, when migration created
        /// </summary>
        public abstract DateTime MigrationDate { get; }
        /// <summary>
        /// Frendly migration name
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Migration logic
        /// </summary>
        /// <param name="database">Database fot applying changes</param>
        /// <returns></returns>
        public abstract Task DoChanges(IMongoDatabase database);
    }
}
