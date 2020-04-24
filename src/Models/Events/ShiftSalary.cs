using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models.Events
{
    public class ShiftSalary : SalaryModel
    {
        public Guid ShiftId { get; set; }
    }
}
