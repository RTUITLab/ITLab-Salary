using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ITLab.Salary.Database;
using ITLab.Salary.Models;
using ITLab.Salary.PublicApi.Request.Salary;
using ITLab.Salary.PublicApi.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ITLab.Salary.Backend.Controllers
{
    [Route("salary/event")]
    [ApiController]
    public class EventSalaryController : ControllerBase
    {
        private readonly SalaryContext salaryContext;
        private readonly ILogger<EventSalaryController> logger;

        public EventSalaryController(
            SalaryContext salaryContext,
            ILogger<EventSalaryController> logger)
        {
            this.salaryContext = salaryContext;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventSalaryView>>> GetList()
        {
            return (await(await salaryContext.EventSalary.FindAsync(Builders<EventSalary>.Filter.Empty)
                .ConfigureAwait(false)).ToListAsync().ConfigureAwait(false)).Select(s => new EventSalaryView
            {
                Id = s.Id,
                AuthorId = s.AuthorId,
                Count = s.Count,
                Created = s.Created,
                Description = s.Description,
                EventId = s.EventId
            }).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<EventSalaryView>> AddEventSalary([FromBody] EventSalaryCreate createRequest)
        {
            try
            {
                var es = new EventSalary
                {
                    AuthorId = Guid.NewGuid(),
                    Created = DateTime.UtcNow,
                    Count = createRequest.Count,
                    Description = createRequest.Description,
                    EventId = createRequest.EventId
                };
                await salaryContext.EventSalary.InsertOneAsync(es).ConfigureAwait(false);
                return new EventSalaryView
                {
                    Id = es.Id,
                    AuthorId = es.AuthorId,
                    Count = es.Count,
                    Created = es.Created,
                    Description = es.Description,
                    EventId = es.EventId
                };
            } catch (MongoWriteException ex) when
            (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                var msd = new ModelStateDictionary();
                msd.AddModelError("eventId", "Salary for that event exists");
                return BadRequest(msd);
            }
        }

        
    }
}
