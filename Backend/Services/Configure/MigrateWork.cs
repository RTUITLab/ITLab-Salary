using ITLab.Salary.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RTUITLab.AspNetCore.Configure.Configure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Services.Configure
{
    public class MigrateWork : IConfigureWork
    {
        private const string CantApplyMigrationsMessage = "Can't apply migrations";
        private readonly SalaryDbContext dbContext;
        private readonly ILogger<MigrateWork> logger;

        public MigrateWork(
            SalaryDbContext dbContext,
            ILogger<MigrateWork> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }
        public async Task Configure(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    logger.LogInformation($"Applying migrations, try {i}");
                    await dbContext.Database.MigrateAsync().ConfigureAwait(false);
                    logger.LogInformation($"Migrations applied");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, CantApplyMigrationsMessage);
                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }
    }
}
