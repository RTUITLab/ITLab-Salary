using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class RootSalaryView : SalaryView
    {
        public Guid AuthorId { get; set; }
        public DateTime ModificationDate { get; set; }
    }
}
