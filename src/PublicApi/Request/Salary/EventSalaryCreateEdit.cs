using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Request.Salary
{
    public class EventSalaryCreateEdit : SalaryInfo
    {
        public List<ShiftSalaryEdit> ShiftSalaries { get; set; }
        public List<PlaceSalaryEdit> PlaceSalaries { get; set; }
    }
}
