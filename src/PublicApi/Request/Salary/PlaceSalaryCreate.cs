using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Request.Salary
{
    public class PlaceSalaryCreate : SalaryInfo
    {
        public Guid PlaceId { get; set; }
    }
}
