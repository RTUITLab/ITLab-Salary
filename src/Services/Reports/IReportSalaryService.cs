using ITLab.Salary.PublicApi.Response.Report;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Reports
{
    public interface IReportSalaryService
    {
        Task<List<ReportUserSalaryCompactView>> GetReportSalaryForUser(Guid userId);
    }
}
