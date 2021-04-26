using AutoMapper;
using ITLab.Salary.Database;
using ITLab.Salary.Models.Reports;
using ITLab.Salary.PublicApi.Response.Report;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ITLab.Salary.Services.Reports
{
    public class WithReportsApiSalaryService : IReportSalaryService
    {
        public const string HTTP_CLIENT_NAME = nameof(WithReportsApiSalaryService) + nameof(HTTP_CLIENT_NAME);
        private readonly ReportSalaryContext reportSalaryContext;
        private readonly IMapper mapper;
        private readonly ILogger<WithReportsApiSalaryService> logger;
        private HttpClient httpClient;

        public WithReportsApiSalaryService(
            ReportSalaryContext reportSalaryContext,
            IMapper mapper, 
            IHttpClientFactory httpClientFactory,
            ILogger<WithReportsApiSalaryService> logger)
        {
            this.httpClient = httpClientFactory.CreateClient(HTTP_CLIENT_NAME);
            this.reportSalaryContext = reportSalaryContext;
            this.mapper = mapper;
            this.logger = logger;
        }
        public Task<List<ReportUserSalaryFullView>> GetReportSalaryForUser(Guid userId)
        {

            throw new NotImplementedException();
        }

        public async Task<List<ReportUserSalaryFullView>> GetReportSalaryForUser(Guid userId, string invokerAccessToken)
        {
            var expression = mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<ReportUserSalary, ReportUserSalaryFullView>();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invokerAccessToken);
            var reportsListJson = await httpClient.GetStringAsync("");
            logger.LogInformation($"Returned from reports: {reportsListJson}");
            var reportsList = JsonSerializer.Deserialize<List<TempReportModel>>(reportsListJson);
            logger.LogInformation($"Deserialize: {JsonSerializer.Serialize(reportsList)}");

            var targetReportsIds = reportsList
                .Where(r => r.Assignees.Implementer == userId)
                .Select(r => r.Id)
                .ToList();

            return await reportSalaryContext.GetById(expression, targetReportsIds);
        }

        private class TempReportModel
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("assignees")]
            public TempAssigneesModel Assignees { get; set; }
        }
        private class TempAssigneesModel
        {
            [JsonPropertyName("implementer")]
            public Guid Implementer { get; set; }
        }
    }
}
