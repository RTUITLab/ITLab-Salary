﻿using ITLab.Salary.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Events
{
    public class SelfReferencedEventsService : IEventsService
    {
        private readonly EventSalaryContext eventSalaryContext;

        public SelfReferencedEventsService(EventSalaryContext eventSalaryContext)
        {
            this.eventSalaryContext = eventSalaryContext;
        }

        public Task<List<Guid>> GetEventIdsInRange(DateTime? begin, DateTime? end)
        {
            return eventSalaryContext.GetAll<Guid>(es => true, es => es.EventId);
        }
    }
}
