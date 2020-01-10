using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.MongoMigrations
{
    /// <summary>
    /// Record about migration for save in special collection
    /// </summary>
    public class MongoMigrationRecord
    {
        /// <summary>
        /// Unique id of record
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        /// <summary>
        /// Unique migration Id
        /// </summary>
        public Guid MigrationId { get; set; }
        /// <summary>
        /// Date, when migration created
        /// </summary>
        public DateTime MigrationDate { get; set; }
        /// <summary>
        /// Frendly migration name
        /// </summary>
        public string Name { get; set; }
    }
}
