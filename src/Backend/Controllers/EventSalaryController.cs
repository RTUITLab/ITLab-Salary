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
    [Produces("application/json")]
    [Route("salary/event")]
    [ApiController]
    public class EventSalaryController : ControllerBase
    {
        private readonly SalaryContext salaryContext;
        private readonly IMapper mapper;
        private readonly ILogger<EventSalaryController> logger;

        public EventSalaryController(
            SalaryContext salaryContext,
            IMapper mapper,
            ILogger<EventSalaryController> logger)
        {
            this.salaryContext = salaryContext;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventSalaryCompactView>>> GetList()
        {
            return await salaryContext
                .GetAll(es => new EventSalaryCompactView { Count = es.Count, EventId = es.EventId })
                .ConfigureAwait(false);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [HttpGet("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> GetOne(Guid eventId)
        {
            var finded = await salaryContext.GetOneOrDefault(eventId).ConfigureAwait(false);
            if (finded == null)
            {
                return NotFound();
            }
            return mapper.Map<EventSalaryFullView>(finded);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<EventSalaryFullView>> AddEventSalary([FromBody] EventSalaryCreate createRequest)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var es = mapper.Map<EventSalary>(createRequest);
                await salaryContext.AddNewEventSalary(es, authorId).ConfigureAwait(false);
                return mapper.Map<EventSalaryFullView>(es);
            }
            catch (MongoWriteException ex) when
                  (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                var msd = new ModelStateDictionary();
                msd.AddModelError("eventId", "Salary for that event exists");
                return BadRequest(msd);
            }
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        [HttpPut("{eventId}")]
        public async Task<ActionResult<EventSalaryFullView>> ChangeEventSalaryInfo(Guid eventId, [FromBody] SalaryInfo info)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                await salaryContext.ChangeEventSalaryInfo(eventId, salary, authorId).ConfigureAwait(false);
                return await GetOne(eventId).ConfigureAwait(false);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
        [HttpPut("{eventId}/{shiftId}")]
        public async Task<ActionResult<EventSalaryFullView>> AddShiftToEventSalary(Guid eventId, Guid shiftId, [FromBody] SalaryInfo info)
        {
            try
            {
                var authorId = Guid.NewGuid();
                var salary = mapper.Map<Models.Salary>(info);
                await salaryContext.AddShiftToEventSalary(eventId, shiftId, salary, authorId).ConfigureAwait(false);
                return await GetOne(eventId).ConfigureAwait(false);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
