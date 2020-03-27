using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Request.Salary
{
    public class ShiftSalaryEdit : SalaryInfo
    {
        public Guid ShiftId { get; set; }
    }
}
