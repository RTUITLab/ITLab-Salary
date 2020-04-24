using AutoMapper;
using AutoMapper.QueryableExtensions;
using ITLab.Salary.Database;
using ITLab.Salary.Models.Events;
using ITLab.Salary.PublicApi.Response;
using ITLab.Salary.Services.Events.Remote;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Events
{
    public class EventSalaryService : IEventSalaryService
    {
        private readonly EventSalaryContext eventSalaryContext;
        private readonly IEventsService eventsService;
        private readonly IMapper mapper;

        public EventSalaryService(
            EventSalaryContext eventSalaryContext,
            IEventsService eventsService,
            IMapper mapper
            )
        {
            this.eventSalaryContext = eventSalaryContext;
            this.eventsService = eventsService;
            this.mapper = mapper;
        }

        public async Task<List<EventSalaryCompactView>> Get(DateTime? begin, DateTime? end)
        {
            var targetIds = await eventsService.GetEventIdsInRange(begin, end);
            var mapExpression = mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<EventSalary, EventSalaryCompactView>();
            return await eventSalaryContext.GetAll(
                es => targetIds.Contains(es.EventId),
                mapExpression);
        }
    }
}
