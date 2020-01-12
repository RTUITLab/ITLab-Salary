using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class ShiftSalaryView : SalaryView
    {
        public Guid ShiftId { get; set; }
    }
}
