using ITLab.Salary.Backend.Models.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Services
{
    /// <summary>
    /// Helper to generate testing Bearer tokens
    /// </summary>
    public static class JwtTestsHelper
    {
        /// <summary>
        /// Key for create and check test Bearer token
        /// </summary>
        /// <param name="key">Secret key</param>
        /// <returns></returns>
        public static SecurityKey IssuerSigningKey(string key)
            => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        /// <summary>
        /// Generate test jwt token for admin with id <see cref="JwtOptions.DebugAdminUserId"/>
        /// </summary>
        /// <param name="jwtOptions"></param>
        /// <returns></returns>
        public static string DebugAdminToken(JwtOptions jwtOptions)
        {
            jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
            var credentials = new SigningCredentials(IssuerSigningKey(jwtOptions.DebugKey), SecurityAlgorithms.HmacSha256);

            var claims = new Claim[] {
                new Claim("sub", jwtOptions.DebugAdminUserId.ToString()),
                new Claim("aud", jwtOptions.Audience),
                new Claim("scope", jwtOptions.Scope)
            };
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
