using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Authorization
{
    /// <summary>
    /// Names for policy
    /// </summary>
    public static class PolicyNames
    {
        /// <summary>
        /// 
        /// </summary>
        public const string SalaryAdmin = nameof(SalaryAdmin);
        /// <summary>
        /// 
        /// </summary>
        public const string ReportsAdmin = nameof(ReportsAdmin);

        /// <summary>
        /// Add <see cref="PolicyNames.ReportsAdmin"/> policy, require reports.admin
        /// </summary>
        /// <param name="options"></param>
        public static void AddReportsAdminPolicy(this AuthorizationOptions options)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            options.AddPolicy(PolicyNames.ReportsAdmin, policy => policy.RequireClaim("itlab", "reports.admin"));
        }

        /// <summary>
        /// Add <see cref="PolicyNames.SalaryAdmin"/> policy, require salary.admin
        /// </summary>
        /// <param name="options"></param>
        public static void AddSalaryAdminPolicy(this AuthorizationOptions options)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));

            options.AddPolicy(PolicyNames.SalaryAdmin, policy => policy.RequireClaim("itlab", "salary.admin"));
        }
    }
}
