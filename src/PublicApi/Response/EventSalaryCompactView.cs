using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class EventSalaryCompactView
    {
        public Guid EventId { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }
    }
}
