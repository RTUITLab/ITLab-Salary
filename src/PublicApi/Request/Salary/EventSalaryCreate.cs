using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Request.Salary
{
    public class EventSalaryCreate : SalaryCreate
    {
        public Guid EventId { get; set; }
    }
}
