using ITLab.Salary.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.MongoMigrations
{
    public class FirstMigration : MongoMigration
    {
        public override DateTime MigrationDate => DateTime.Parse("1/9/2020 11:18:08 AM", null, DateTimeStyles.AssumeUniversal);

        public override string Name => "First migration";

        public override async Task DoChanges(IMongoDatabase database)
        {
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
}
