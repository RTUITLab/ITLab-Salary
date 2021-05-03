using ITLab.Salary.PublicApi.Response.Report;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Reports
{
    public interface IReportSalaryService
    {
        [Obsolete("Created as hot fix for admin users")]
        Task<List<ReportUserSalaryFullView>> GetAllSalaryInfo();
        Task<List<ReportUserSalaryFullView>> GetReportSalaryForUser(Guid userId);
        [Obsolete("Waiting to rewrite reports service. Use direct access to reports service fron salary, instead using client access token")]
        Task<List<ReportUserSalaryFullView>> GetReportSalaryForUser(Guid userId, string invokerAccessToken);
    }
}
