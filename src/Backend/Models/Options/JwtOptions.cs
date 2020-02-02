using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Models.Options
{
    /// <summary>
    /// Options to configure JWT
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }
        /// <summary>
        /// Single valid audience value for any received OpenIdConnect token
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// Scope for salaey API
        /// </summary>
        public string Scope { get; set; }
        /// <summary>
        /// Key for signing debug tokens
        /// </summary>
        public string DebugKey { get; set; }
        /// <summary>
        /// Tests admin user id
        /// </summary>
        public Guid DebugAdminUserId { get; set; }
    }
}
