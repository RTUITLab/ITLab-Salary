using AutoMapper;
using ITLab.Salary.Backend.Authorization;
using ITLab.Salary.Database;
using ITLab.Salary.Models.Reports;
using ITLab.Salary.PublicApi.Request.Salary;
using ITLab.Salary.PublicApi.Response.Report;
using ITLab.Salary.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMapper mapper;
        private readonly ILogger<ReportSalaryController> logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reportSalaryContext"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public ReportSalaryController(
            ReportSalaryContext reportSalaryContext,
            IMapper mapper,
            ILogger<ReportSalaryController> logger)
        {
            this.reportSalaryContext = reportSalaryContext;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Get list of report salary
        /// </summary>
        /// <param name="begin">Biggest end time. If not defined end time equals infinity</param>
        /// <param name="end">Smallest begin time. If not defined begin time equals zero</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportUserSalaryCompactView>>> GetList(DateTime? begin, DateTime? end)
        {
            var list = await reportSalaryContext.GetAll().ConfigureAwait(false);
            return list.Select(s => new ReportUserSalaryCompactView
            {
                ReportId = s.ReportId,
                Count = s.Count,
                Description = s.Description
            }).ToList();
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
        [Authorize(Policy = PolicyNames.ReportsAdmin)]
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
