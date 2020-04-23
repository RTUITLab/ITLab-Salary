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
        public const string ReportsAdmin = nameof(ReportsAdmin);
    }
}
