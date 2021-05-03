using AutoMapper;
using AutoMapper.QueryableExtensions;
using ITLab.Salary.Database;
using ITLab.Salary.Models.Reports;
using ITLab.Salary.PublicApi.Response.Report;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Reports
{
    public class GetAllReportSalaryService : IReportSalaryService
    {
        private readonly ReportSalaryContext reportSalaryContext;
        private readonly IMapper mapper;
        private readonly ILogger<GetAllReportSalaryService> logger;

        public GetAllReportSalaryService(
            ReportSalaryContext reportSalaryContext, 
            IMapper mapper,
            ILogger<GetAllReportSalaryService> logger)
        {
            logger.LogWarning("using get all salary service");
            this.reportSalaryContext = reportSalaryContext;
            this.mapper = mapper;
            this.logger = logger;
        }

        public Task<List<ReportUserSalaryFullView>> GetAllSalaryInfo()
        {
            var expression = mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<ReportUserSalary, ReportUserSalaryFullView>();
            return reportSalaryContext.GetAll(expression);
        }

        public Task<List<ReportUserSalaryFullView>> GetReportSalaryForUser(Guid userId)
        {
            return GetAllSalaryInfo();
        }

        public Task<List<ReportUserSalaryFullView>> GetReportSalaryForUser(Guid userId, string invokerAccessToken)
        {
            return GetAllSalaryInfo();
        }
    }
}
