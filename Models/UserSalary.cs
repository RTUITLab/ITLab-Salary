using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class UserSalary : PlaceSalary
    {
        public Guid UserId { get; set; }
        public DateTime Approved { get; set; }
        public Guid ApproverId { get; set; }
    }
}
