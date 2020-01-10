using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.PublicApi.Response
{
    public class PlaceSalaryView : SalaryView
    {
        public Guid PlaceId { get; set; }
    }
}
