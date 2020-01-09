using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.MongoMigrations
{
    public abstract class MongoMigration
    {
        public abstract DateTime MigrationDate { get; }
        public abstract string Name { get; }
        public abstract Task DoChanges(IMongoDatabase database);
    }
}
