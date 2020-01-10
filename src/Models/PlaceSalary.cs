using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class PlaceSalary : MetaSalary
    {
        public Guid PlaceId { get; set; }
        public List<UserSalary> UserSalaries { get; set; }
    }
}
