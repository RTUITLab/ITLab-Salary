﻿using ITLab.Salary.Models.Reports;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Database
{
    public class ReportSalaryContext : MongoDbContext<ReportUserSalary>
    {
        public ReportSalaryContext(IDbFactory dbFactory) : base (dbFactory)
        {
        }

        public Task<List<T>> GetAll<T>(Expression<Func<ReportUserSalary, T>> projection)
        {
            return Collection.Find(Builders<ReportUserSalary>.Filter.Empty).Project(projection).ToListAsync();
        }

        public Task<List<T>> GetById<T>(Expression<Func<ReportUserSalary, T>> projection, List<string> targetIds)
        {
            return Collection.Find(Builders<ReportUserSalary>.Filter.Where(r => targetIds.Contains(r.ReportId))).Project(projection).ToListAsync();
        }

        public async Task<ReportUserSalary> UpdateReportUserSalary(string reportId, ReportUserSalary reportUserSalary, Guid approverId)
        {
            reportUserSalary = reportUserSalary ?? throw new ArgumentNullException(nameof(reportUserSalary));
            var now = DateTime.UtcNow;
            reportUserSalary.ReportId = reportId;
            reportUserSalary.AuthorId = approverId;
            reportUserSalary.Created = reportUserSalary.ModificationDate = now;

            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<ReportUserSalary>.Filter.Where(es => es.ReportId == reportId),
                Builders<ReportUserSalary>.Update
                    .Set(es => es.ReportId, reportUserSalary.ReportId)
                    .SetOnInsert(es => es.Created, reportUserSalary.Created)
                    .Set(es => es.Count, reportUserSalary.Count)
                    .Set(es => es.Description, reportUserSalary.Description)
                    .Set(es => es.ModificationDate, reportUserSalary.ModificationDate)
                    .Set(es => es.AuthorId, reportUserSalary.AuthorId),
                new FindOneAndUpdateOptions<ReportUserSalary, ReportUserSalary> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
                ).ConfigureAwait(false);

            await SaveToHistory(updated).ConfigureAwait(false);

            return updated;
        }
    }
}
