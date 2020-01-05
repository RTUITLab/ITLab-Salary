using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class EventSalaryView
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
        public Guid AuthorId { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public Guid EventId { get; set; }
    }
}
