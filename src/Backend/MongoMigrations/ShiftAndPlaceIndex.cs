using ITLab.Salary.Database;
using ITLab.Salary.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.MongoMigrations
{
    public class ShiftAndPlaceIndex : MongoMigration
    {
        public override DateTime MigrationDate => DateTime.Parse("1/10/2020 11:55:30 AM", null, DateTimeStyles.AssumeUniversal);

        public override string Name => "Shift and place index";

        public override async Task DoChanges(IMongoDatabase database)
        {
            var result = await database.GetCollection<EventSalary>(SalaryContext.EventSalaryCollectionName)
                .Indexes
                .CreateOneAsync(
                new CreateIndexModel<EventSalary>(
                    Builders<EventSalary>
                        .IndexKeys
                        .Descending($"{nameof(EventSalary.ShiftSalaries)}.{nameof(ShiftSalary.ShiftId)}"),
                    new CreateIndexOptions<EventSalary> { Unique = true }
                )
            ).ConfigureAwait(false);
            result = await database.GetCollection<EventSalary>(SalaryContext.EventSalaryCollectionName)
                .Indexes
                .CreateOneAsync(
                new CreateIndexModel<EventSalary>(
                    Builders<EventSalary>
                        .IndexKeys
                        .Descending($"{nameof(EventSalary.ShiftSalaries)}.{nameof(ShiftSalary.PlaceSalaries)}.{nameof(PlaceSalary.PlaceId)}"),
                    new CreateIndexOptions<EventSalary> { Unique = true }
                )
            ).ConfigureAwait(false);
            result = await database.GetCollection<EventSalary>(SalaryContext.EventSalaryCollectionName)
                .Indexes
                .CreateOneAsync(
                new CreateIndexModel<EventSalary>(
                    Builders<EventSalary>
                        .IndexKeys
                        .Descending($"{nameof(EventSalary.ShiftSalaries)}.{nameof(ShiftSalary.PlaceSalaries)}.{nameof(PlaceSalary.UserSalaries)}.{nameof(UserSalary.UserId)}"),
                    new CreateIndexOptions<EventSalary> { Unique = true }
                )
            ).ConfigureAwait(false);
        }
    }
}
