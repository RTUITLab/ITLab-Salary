using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models.Events
{
    public class PlaceSalary : SalaryModel
    {
        public Guid PlaceId { get; set; }
    }
}
