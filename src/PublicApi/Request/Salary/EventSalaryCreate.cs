using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Request.Salary
{
    public class EventSalaryCreate : SalaryInfo
    {
        public Guid EventId { get; set; }
        public List<ShiftSalaryCreate> ShiftSalaries { get; set; }
    }
}
