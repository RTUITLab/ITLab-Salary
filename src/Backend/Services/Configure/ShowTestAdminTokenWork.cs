using ITLab.Salary.Backend.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RTUITLab.AspNetCore.Configure.Configure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Services.Configure
{
    /// <summary>
    /// Service to show testing admin bearer token
    /// </summary>
    public class ShowTestAdminTokenWork : IConfigureWork
    {
        private readonly ILogger<ShowTestAdminTokenWork> logger;
        private readonly IOptions<JwtOptions> jwtOptions;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="jwtOptions"></param>
        public ShowTestAdminTokenWork(ILogger<ShowTestAdminTokenWork> logger, IOptions<JwtOptions> jwtOptions)
        {
            this.logger = logger;
            this.jwtOptions = jwtOptions;
        }
        /// <summary>
        /// Show token
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Configure(CancellationToken cancellationToken)
        {
            logger.LogInformation(JwtTestsHelper.DebugAdminToken(jwtOptions.Value));
            return Task.CompletedTask;
        }
    }
}
