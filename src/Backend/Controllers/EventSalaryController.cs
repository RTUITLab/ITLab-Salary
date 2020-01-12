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
    [Route("salary/v{version:apiVersion}/event")]
    [ApiController]
    public class EventSalaryController : ControllerBase
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
        [HttpGet("{eventId}", Name = "GetOneByEventID")]
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
        /// Add new event salary record
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="apiVersion"></param>
        /// <param name="createRequest">Info about event salary</param>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">Event salary for thayt event exists</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Dictionary<string, string>))]
        [HttpPost("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> AddEventSalary(
            Guid eventId, 
            ApiVersion apiVersion,
            [FromBody] EventSalaryCreate createRequest)
        {
            apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
            try
            {
                logger.LogWarning("using mock author");
                var authorId = Guid.NewGuid();
                var es = mapper.Map<EventSalary>(createRequest);
                await eventSalaryContext.AddNew(eventId, es, authorId).ConfigureAwait(false);
                return CreatedAtRoute("GetOneByEventID",  new { eventId = es.EventId, version = apiVersion.ToString() }, mapper.Map<EventSalaryFullView>(es));
            }
            catch (MongoWriteException ex) when
                  (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                var msd = new Dictionary<string, string>
                {
                    ["eventId"] = "Salary for that event exists"
                };
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
        public async Task<ActionResult<EventSalaryFullView>> UpdateEventSalaryInfo(
            Guid eventId,
            [FromBody] SalaryInfo info)
        {
            try
            {
                logger.LogWarning("using mock author");
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                var updated = await eventSalaryContext.UpdateEvenInfo(eventId, salary, authorId).ConfigureAwait(false);
                return mapper.Map<EventSalaryFullView>(updated);
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }


        /// <summary>
        /// Add new shift salary to event salary
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
        [HttpPost("{eventId}/shift/{shiftId}")]
        public async Task<ActionResult<EventSalaryFullView>> AddShiftToEventSalary(Guid eventId, Guid shiftId, [FromBody] SalaryInfo info)
        {
            try
            {
                logger.LogWarning("using mock author");
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                var updated = await eventSalaryContext.AddShift(eventId, shiftId, salary, authorId).ConfigureAwait(false);
                return mapper.Map<EventSalaryFullView>(updated);
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
        [HttpPut("{eventId}/shift/{shiftId}")]
        public async Task<ActionResult<EventSalaryFullView>> UpdateShiftSalaryInfo(Guid eventId, Guid shiftId, [FromBody] SalaryInfo info)
        {
            try
            {
                logger.LogWarning("using mock author");
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                var updated = await eventSalaryContext.UpdateShiftInfo(eventId, shiftId, salary, authorId).ConfigureAwait(false);
                return mapper.Map<EventSalaryFullView>(updated);
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }

        /// <summary>
        /// Add new place salary to event salary
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="placeId">Target place id</param>
        /// <param name="info">New Place salary info</param>
        /// <response code="200">Returns the newly created item</response>
        /// <response code="400">Place id exists on that event</response>
        /// <response code="404">Event salary not found </response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpPost("{eventId}/place/{placeId}")]
        public async Task<ActionResult<EventSalaryFullView>> AddPlaceToEventSalary(Guid eventId, Guid placeId, [FromBody] SalaryInfo info)
        {
            try
            {
                logger.LogWarning("using mock author");
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                var updated = await eventSalaryContext.AddPlace(eventId, placeId, salary, authorId).ConfigureAwait(false);
                return mapper.Map<EventSalaryFullView>(updated);
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
        /// Update place salary info of event salary
        /// </summary>
        /// <param name="eventId">Target event id</param>
        /// <param name="placeId">Target place id</param>
        /// <param name="info">New Shift salary info</param>
        /// <response code="200">Returns the updated item</response>
        /// <response code="404">Event salary not found</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpPut("{eventId}/place/{placeId}")]
        public async Task<ActionResult<EventSalaryFullView>> UpdatPlaceSalaryInfo(
            Guid eventId, 
            Guid placeId,
            [FromBody] SalaryInfo info)
        {
            try
            {
                logger.LogWarning("using mock author");
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                var updated = await eventSalaryContext.UpdatePlaceInfo(eventId, placeId, salary, authorId).ConfigureAwait(false);
                return mapper.Map<EventSalaryFullView>(updated);
            }
            catch (NotFoundException nfe)
            {
                return NotFound(nfe.Message);
            }
        }
    }
}
