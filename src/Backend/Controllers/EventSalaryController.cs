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
    [Produces("application/json")]
    [Route("salary/event")]
    [ApiController]
    public class EventSalaryController : ControllerBase
    {
        private readonly SalaryContext salaryContext;
        private readonly IMapper mapper;
        private readonly ILogger<EventSalaryController> logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="salaryContext"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public EventSalaryController(
            SalaryContext salaryContext,
            IMapper mapper,
            ILogger<EventSalaryController> logger)
        {
            this.salaryContext = salaryContext;
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
            return await salaryContext
                .GetAll(es => new EventSalaryCompactView { Count = es.Count, EventId = es.EventId })
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
            var finded = await salaryContext.GetOneOrDefault(eventId).ConfigureAwait(false);
            if (finded == null)
            {
                return NotFound("NotFound event salary");
            }
            return mapper.Map<EventSalaryFullView>(finded);
        }

        /// <summary>
        /// Add new event salary record
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="createRequest">Info about event salary</param>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">Event salary for thayt event exists</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
        [HttpPost("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> AddEventSalary(Guid eventId, [FromBody] EventSalaryCreate createRequest)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var es = mapper.Map<EventSalary>(createRequest);
                await salaryContext.AddNewEventSalary(eventId, es, authorId).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetOne), new { eventId = es.EventId }, mapper.Map<EventSalaryFullView>(es));
            }
            catch (MongoWriteException ex) when
                  (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                var msd = new ModelStateDictionary();
                msd.AddModelError("eventId", "Salary for that event exists");
                return BadRequest(msd);
            }
        }
        /// <summary>
        /// Update ONLY event salary info
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="info">Info to update on event level</param>
        /// <response code="200">Returns the updated info</response>
        /// <response code="404">Event salary not found</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpPut("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> UpdateEventSalaryInfo(Guid eventId, [FromBody] SalaryInfo info)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                await salaryContext.UpdateEventSalaryInfo(eventId, salary, authorId).ConfigureAwait(false);
                return await GetOne(eventId).ConfigureAwait(false);
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }


        /// <summary>
        /// Add new shift to event salary
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="shiftId">Target shift id</param>
        /// <param name="info">New Shift salary info</param>
        /// <response code="200">Returns the newly created item</response>
        /// <response code="400">Shift id exists on that event</response>
        /// <response code="404">Event salary not found </response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpPost("{eventId}/{shiftId}")]
        public async Task<ActionResult<EventSalaryFullView>> AddShiftToEventSalary(Guid eventId, Guid shiftId, [FromBody] SalaryInfo info)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                await salaryContext.AddShiftToEventSalary(eventId, shiftId, salary, authorId).ConfigureAwait(false);
                return await GetOne(eventId).ConfigureAwait(false);
            }
            catch (BadRequestException bre)
            {
                return BadRequest(bre.Message);
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }

        /// <summary>
        /// Update shift salary info of event salary
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="shiftId">Target shift id</param>
        /// <param name="info">New Shift salary info</param>
        /// <response code="200">Returns the updated item</response>
        /// <response code="404">Event or shift salary not found</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpPut("{eventId}/{shiftId}")]
        public async Task<ActionResult<EventSalaryFullView>> UpdateShiftSalaryInfo(Guid eventId, Guid shiftId, [FromBody] SalaryInfo info)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                await salaryContext.UpdateShiftSalaryInfo(eventId, shiftId, salary, authorId).ConfigureAwait(false);
                return await GetOne(eventId).ConfigureAwait(false);
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }

    }
}
