using ITLab.Salary.Models.Events;
using ITLab.Salary.Models.Reports;
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
    public class ReportsMigration : MongoMigration
    {
        public override Guid Id => Guid.Parse("D593B002-9B7A-400A-A492-BCCA535B2F72");
        public override DateTime MigrationDate => DateTime.Parse("23.04.2020 11:36:50", null, DateTimeStyles.AssumeUniversal);

        public override string Name => "Reports migration";


        public override async Task DoChanges(IMongoDatabase database)
        {
            database = database ?? throw new ArgumentNullException(nameof(database));
            await database.CreateCollectionAsync(nameof(ReportUserSalary)).ConfigureAwait(false);
            var eventSalaryCollection = database.GetCollection<ReportUserSalary>(nameof(ReportUserSalary));
            var result = await eventSalaryCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<ReportUserSalary>(
                Builders<ReportUserSalary>
                    .IndexKeys
                    .Descending(es => es.ReportId), new CreateIndexOptions<ReportUserSalary> { Unique = true })
            ).ConfigureAwait(false);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
