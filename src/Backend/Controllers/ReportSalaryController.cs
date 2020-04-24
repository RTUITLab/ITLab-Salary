using AutoMapper;
using AutoMapper.QueryableExtensions;
using ITLab.Salary.Backend.Authorization;
using ITLab.Salary.Database;
using ITLab.Salary.Models.Reports;
using ITLab.Salary.PublicApi.Request.Salary;
using ITLab.Salary.PublicApi.Response.Report;
using ITLab.Salary.Services.Reports;
using ITLab.Salary.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Controllers
{
    /// <summary>
    /// Manage event salary
    /// </summary>
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/salary/v{version:apiVersion}/report")]
    public class ReportSalaryController : AuthorizedController
    {
        private readonly ReportSalaryContext reportSalaryContext;
        private readonly IReportSalaryService reportSalaryService;
        private readonly IAuthorizationService authorizationService;
        private readonly IMapper mapper;
        private readonly ILogger<ReportSalaryController> logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reportSalaryContext"></param>
        /// <param name="reportSalaryService"></param>
        /// <param name="authorizationService"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public ReportSalaryController(
            ReportSalaryContext reportSalaryContext,
            IReportSalaryService reportSalaryService,
            IAuthorizationService authorizationService,
            IMapper mapper,
            ILogger<ReportSalaryController> logger)
        {
            this.reportSalaryContext = reportSalaryContext;
            this.reportSalaryService = reportSalaryService;
            this.authorizationService = authorizationService;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Get list of report salary
        /// </summary>
        /// <returns></returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ReportUserSalaryCompactView>>> GetList(Guid userId)
        {
            if (UserId != userId)
            {
                var adminAuthorize = await authorizationService.AuthorizeAsync(User, PolicyNames.SalaryAdmin).ConfigureAwait(false);
                if (!adminAuthorize.Succeeded)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, "require salary admin permissons");
                }
            }

            return await reportSalaryService.GetReportSalaryForUser(UserId).ConfigureAwait(false);
        }

        ///// <summary>
        ///// Update or Create event salary info
        ///// </summary>
        ///// <param name="reportId">Target event id</param>
        ///// <param name="info">Event salary info</param>
        ///// <response code="200">Returns the updated info</response>
        ///// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut("{reportId}")]
        [Authorize(Policy = PolicyNames.SalaryAdmin)]
        public async Task<ActionResult<ReportUserSalaryFullView>> UpdateEventSalaryInfo(
            string reportId,
            [FromBody] ReportUserSalaryEdit info)
        {
            var salary = mapper.Map<ReportUserSalary>(info);
            var updated = await reportSalaryContext.UpdateReportUserSalary(reportId, salary, UserId).ConfigureAwait(false);
            return Ok(mapper.Map<ReportUserSalaryFullView>(updated));
        }
    }
}
