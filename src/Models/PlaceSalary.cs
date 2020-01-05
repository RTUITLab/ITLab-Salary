using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class PlaceSalary : ShiftSalary
    {
        public Guid PlaceId { get; set; }
    }
}
