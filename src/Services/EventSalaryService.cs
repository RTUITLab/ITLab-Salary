using ITLab.Salary.Database;
using ITLab.Salary.PublicApi.Response;
using ITLab.Salary.Services.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Services
{
    public class EventSalaryService : IEventSalaryService
    {
        private readonly EventSalaryContext eventSalaryContext;
        private readonly IEventsService eventsService;

        public EventSalaryService(
            EventSalaryContext eventSalaryContext,
            IEventsService eventsService
            )
        {
            this.eventSalaryContext = eventSalaryContext;
            this.eventsService = eventsService;
        }

        public async Task<List<EventSalaryCompactView>> Get(DateTime? begin, DateTime? end)
        {
            var targetIds = await eventsService.GetEventIdsInRange(begin, end);
            return await eventSalaryContext.GetAll(
                es => targetIds.Contains(es.EventId),
                es => new EventSalaryCompactView
                {
                    Count = es.Count,
                    Description = es.Description,
                    EventId = es.EventId
                });
        }
    }
}
