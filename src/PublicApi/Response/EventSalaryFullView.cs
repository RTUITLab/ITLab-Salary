using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class EventSalaryFullView : RootSalaryView
    {
        public Guid EventId { get; set; }
        public DateTime Created { get; set; }
        public List<ShiftSalaryView> ShiftSalaries { get; set; }
        public List<PlaceSalaryView> PlaceSalaries { get; set; }
    }
}
