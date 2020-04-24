using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models.Events
{
    public class EventSalary : SalaryEntity
    {
        public Guid EventId { get; set; }
        public List<ShiftSalary> ShiftSalaries { get; set; }
        public List<PlaceSalary> PlaceSalaries { get; set; }
    }
}
