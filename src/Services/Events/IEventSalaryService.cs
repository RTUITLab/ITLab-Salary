using ITLab.Salary.PublicApi.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Events
{
    public interface IEventSalaryService
    {
        Task<List<EventSalaryCompactView>> Get(DateTime? begin, DateTime? end);
    }
}
