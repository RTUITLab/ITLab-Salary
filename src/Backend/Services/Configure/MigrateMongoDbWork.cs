using ITLab.Salary.Backend.MongoMigrations;
using ITLab.Salary.Database;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using RTUITLab.AspNetCore.Configure.Configure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Services.Configure
{
    public class MigrateMongoDbWork : IConfigureWork
    {
        private readonly ILogger<MigrateMongoDbWork> logger;
        private readonly SalaryContext salaryContext;

        public MigrateMongoDbWork(
            ILogger<MigrateMongoDbWork> logger,
            SalaryContext salaryContext)
        {
            this.logger = logger;
            this.salaryContext = salaryContext;
        }

        public async Task Configure(CancellationToken cancellationToken)
        {
            var appliedMigrations = await GetAppliedMigrations(salaryContext.Database).ConfigureAwait(false);
            logger.LogInformation($"Already applied {appliedMigrations.Count} migrations");
            var migrations = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(MongoMigration).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<MongoMigration>()
                .Where(m => appliedMigrations.All(am => am.Name != m.Name))
                .OrderBy(m => m.MigrationDate)
                .ToArray();

            foreach (var migration in migrations)
            {
                logger.LogInformation($"apply migration {migration.Name}");
                await migration.DoChanges(salaryContext.Database).ConfigureAwait(false);
                await SaveMigrationRecord(salaryContext.Database, migration).ConfigureAwait(false);
            }
        }

        private Task SaveMigrationRecord(IMongoDatabase database, MongoMigration migration)
        {
            var migrations = database.GetCollection<MongoMigrationRecord>("__Migrations");
            return migrations.InsertOneAsync(new MongoMigrationRecord { Name = migration.Name, MigrationDate = migration.MigrationDate });
        }

        private async Task<List<MongoMigrationRecord>> GetAppliedMigrations(IMongoDatabase database)
        {
            var migrations = database.GetCollection<MongoMigrationRecord>("__Migrations");
            var total = await migrations.Find(Builders<MongoMigrationRecord>.Filter.Empty).ToListAsync().ConfigureAwait(false);
            return total;
        }
    }
}
