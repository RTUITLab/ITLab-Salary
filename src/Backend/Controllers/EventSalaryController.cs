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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ITLab.Salary.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventSalaryController : ControllerBase
    {
        private readonly SalaryDbContext db;
        private readonly ILogger<EventSalaryController> logger;

        public EventSalaryController(
            SalaryDbContext salaryDbContext,
            ILogger<EventSalaryController> logger)
        {
            this.db = salaryDbContext;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventSalaryView>>> GetAsync()
        {
            return await db.EventSalaries.AsNoTracking().Select(s => new EventSalaryView
            {
                Id = s.Id,
                AuthorId = s.AuthorId,
                Count = s.Count,
                Created = s.Created,
                Description = s.Description,
                EventId = s.EventId
            }).ToListAsync().ConfigureAwait(false);
        }

        // GET: api/EventSalary/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/EventSalary
        [HttpPost]
        public async Task<ActionResult<EventSalaryView>> PostAsync( [FromBody] EventSalaryCreate createRequest)
        {
            try
            {
                var es = new EventSalary
                {
                    AuthorId = Guid.NewGuid(),
                    Created = DateTime.Now,
                    Count = createRequest.Count,
                    Description = createRequest.Description,
                    EventId = createRequest.EventId
                };
                db.EventSalaries.Add(es);
                await db.SaveChangesAsync().ConfigureAwait(false);
                return new EventSalaryView
                {
                    Id = es.Id,
                    AuthorId = es.AuthorId,
                    Count = es.Count,
                    Created = es.Created,
                    Description = es.Description,
                    EventId = es.EventId
                };
            } catch (DbUpdateException ex) when
            (ex.InnerException is PostgresException pe && pe.SqlState == "23505")
            {
                var msd = new ModelStateDictionary();
                msd.AddModelError("eventId", "Salary for that event exists");
                return BadRequest(msd);
            }
        }

        // PUT: api/EventSalary/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
