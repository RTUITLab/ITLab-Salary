using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Events
{
    public class SafeRemoteApiEventsService : IEventsService
    {
        public Task<List<Guid>> GetEventIdsInRange(DateTime? begin, DateTime? end)
        {
            throw new NotImplementedException();
        }
    }
}
