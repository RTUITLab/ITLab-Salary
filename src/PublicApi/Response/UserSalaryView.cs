using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class UserSalaryView : SalaryView
    {
        public DateTime Approved { get; set; }
        public Guid ApproverId { get; set; }
    }
}
