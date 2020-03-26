using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ITLab.Salary.Database;
using ITLab.Salary.Models;
using ITLab.Salary.PublicApi.Request.Salary;
using ITLab.Salary.PublicApi.Response;
using ITLab.Salary.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ITLab.Salary.Backend.Controllers
{
    /// <summary>
    /// Manage event salary
    /// </summary>
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("api/salary/v{version:apiVersion}/event")]
    public class EventSalaryController : AuthorizedController
    {
        private readonly EventSalaryContext eventSalaryContext;
        private readonly IMapper mapper;
        private readonly ILogger<EventSalaryController> logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="eventSalaryContext"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public EventSalaryController(
            EventSalaryContext eventSalaryContext,
            IMapper mapper,
            ILogger<EventSalaryController> logger)
        {
            this.eventSalaryContext = eventSalaryContext;
            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Get all event salary models
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventSalaryCompactView>>> GetList()
        {
            return await eventSalaryContext
                .GetAll(es => new EventSalaryCompactView { Count = es.Count, Description = es.Description, EventId = es.EventId })
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get one full event salary by event id
        /// </summary>
        /// <param name="eventId">event id to search</param>
        /// <response code="200">Returns the finded item</response>
        /// <response code="404">Item not found</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpGet("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> GetOne(Guid eventId)
        {
            var finded = await eventSalaryContext.GetOneOrDefault(eventId).ConfigureAwait(false);
            if (finded == null)
            {
                return NotFound("NotFound event salary");
            }
            return mapper.Map<EventSalaryFullView>(finded);
        }

        /// <summary>
        /// Update or Create event salary info
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="info">Event salary info</param>
        /// <response code="200">Returns the updated info</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> UpdateEventSalaryInfo(
            Guid eventId,
            [FromBody] EventSalaryCreateEdit info)
        {
            try
            {
                var salary = mapper.Map<EventSalary>(info);
                var updated = await eventSalaryContext.UpdateEvenInfo(eventId, salary, UserId).ConfigureAwait(false);
                return Ok(mapper.Map<EventSalaryFullView>(updated));
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }

        /// <summary>
        /// Delete event salary record
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="apiVersion"></param>
        /// <response code="200">If object deleted</response>
        /// <response code="404">Not found object</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpDelete("{eventId}")]
        public async Task<ActionResult> DeleteEventSalary(
            Guid eventId,
            ApiVersion apiVersion)
        {
            apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
            try
            {
                await eventSalaryContext.Delete(eventId, UserId).ConfigureAwait(false);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
