using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Controllers
{
    /// <summary>
    /// Base controller for authorized actions
    /// </summary>
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    public class AuthorizedController : ControllerBase
    {
        /// <summary>
        /// Sender userId
        /// </summary>
        protected Guid UserId
        {
            get
            {
                Console.WriteLine(HttpContext.Request.Headers["Authorization"]);
                return Guid.Parse(HttpContext.User.FindFirst("sub").Value);
            }
        }
    }
}
