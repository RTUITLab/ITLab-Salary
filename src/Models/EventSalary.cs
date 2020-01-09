using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class EventSalary : RootSalary
    {
        public Guid EventId { get; set; }
        public List<ShiftSalary> ShiftSalaries { get; set; }
    }
}
