using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class EventSalaryCompactView : SalaryView
    {
        public Guid EventId { get; set; }
    }
}
