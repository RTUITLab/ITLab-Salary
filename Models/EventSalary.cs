using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class EventSalary : Salary
    {
        public Guid EventId { get; set; }
    }
}
