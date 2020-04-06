using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Events
{
    /// <summary>
    /// Service for events system
    /// </summary>
    public interface IEventsService
    {
        /// <summary>
        /// Returns ids of event
        /// </summary>
        /// <param name="begin">Biggest end time. If not defined end time equals infinity</param>
        /// <param name="end">Smallest begin time. If not defined begin time equals zero</param>
        /// <returns></returns>
        Task<List<Guid>> GetEventIdsInRange(DateTime? begin, DateTime? end);
    }
}
