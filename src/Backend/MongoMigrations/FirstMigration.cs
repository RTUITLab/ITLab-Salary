using ITLab.Salary.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.MongoMigrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// First migration of database
    /// </summary>
    public class FirstMigration : MongoMigration
    {
        public override Guid Id => Guid.Parse("D593B002-9B7A-400A-A492-BCCA535B2F71");
        public override DateTime MigrationDate => DateTime.Parse("1/9/2020 11:18:08 AM", null, DateTimeStyles.AssumeUniversal);

        public override string Name => "First migration";


        public override async Task DoChanges(IMongoDatabase database)
        {
            database = database ?? throw new ArgumentNullException(nameof(database));
            await database.CreateCollectionAsync(nameof(EventSalary)).ConfigureAwait(false);
            var eventSalaryCollection = database.GetCollection<EventSalary>(nameof(EventSalary));
            var result = await eventSalaryCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<EventSalary>(
                Builders<EventSalary>
                    .IndexKeys
                    .Descending(es => es.EventId), new CreateIndexOptions<EventSalary> { Unique = true })
            ).ConfigureAwait(false);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
