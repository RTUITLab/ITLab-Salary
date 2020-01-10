using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class ShiftSalary : MetaSalary
    {
        public Guid ShiftId { get; set; }
        public List<PlaceSalary> PlaceSalaries { get; set; }
    }
}
