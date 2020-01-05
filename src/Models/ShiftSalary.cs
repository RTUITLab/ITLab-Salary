using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class ShiftSalary : EventSalary
    {
        public Guid ShiftId { get; set; }
    }
}
