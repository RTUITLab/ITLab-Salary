using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Request.Salary
{
    public class EventSalaryCreate : SalaryInfo
    {
        public List<ShiftSalaryCreate> ShiftSalaries { get; set; }
    }
}
