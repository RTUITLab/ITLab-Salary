using ITLab.Salary.Models;
using ITLab.Salary.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Database
{
    public class SalaryContext
    {
        public const string EventSalaryCollectionName = nameof(Models.EventSalary);
        public const string EventSalaryHistoryCollectionName = nameof(Models.EventSalary) + "_History";
        private readonly ILogger<SalaryContext> logger;

        public IMongoDatabase Database { get; }
        public SalaryContext(
            string connectionString,
            ILogger<SalaryContext> logger)
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
            var connection = new MongoUrlBuilder(connectionString);
            MongoClient client = new MongoClient(connectionString);
            Database = client.GetDatabase(connection.DatabaseName);
            this.logger = logger;
        }

        private IMongoCollection<EventSalary> EventSalary => Database.GetCollection<EventSalary>(EventSalaryCollectionName);
        private IMongoCollection<EventSalary> EventSalaryHistory => Database.GetCollection<EventSalary>(EventSalaryHistoryCollectionName);

        public Task<List<EventSalary>> GetAll()
        {
            return EventSalary.Find(Builders<EventSalary>.Filter.Empty).ToListAsync();
        }

        public Task<List<T>> GetAll<T>(Expression<Func<EventSalary, T>> projection)
        {
            return EventSalary.Find(Builders<EventSalary>.Filter.Empty).Project(projection).ToListAsync();
        }

        public Task<EventSalary> GetOneOrDefault(Guid eventId)
        {
            return EventSalary.Find(es => es.EventId == eventId).SingleOrDefaultAsync();
        }

        public async Task AddNewEventSalary(EventSalary eventSalary, Guid authorId)
        {
            try
            {
                var now = DateTime.UtcNow;
                eventSalary.Created = eventSalary.ModificationDate = now;
                eventSalary.AuthorId = authorId;
                eventSalary.ShiftSalaries.ForEach(ss =>
                {
                    ss.Created = ss.ModificationDate = now;
                    ss.AuthorId = authorId;
                    ss.PlaceSalaries.ForEach(ps =>
                    {
                        ps.Created = ps.ModificationDate = now;
                        ps.AuthorId = authorId;
                        ps.UserSalaries = null;
                    });
                });
                await EventSalary.InsertOneAsync(eventSalary);
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                await EventSalaryHistory.InsertOneAsync(eventSalary);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Can't insert history record");
            }
        }

        public async Task ChangeEventSalaryInfo(Guid eventId, Models.Salary info, Guid authorId)
        {
            var result = await EventSalary.UpdateOneAsync(
                es => es.EventId == eventId,
                Builders<EventSalary>.Update
                    .Set(es => es.ModificationDate, DateTime.UtcNow)
                    .Set(es => es.AuthorId, authorId)
                    .Set(es => es.Description, info.Description)
                    .Set(es => es.Count, info.Count));
            if (result.ModifiedCount == 0)
                throw new NotFoundException();
        }

        public async Task AddShiftToEventSalary(Guid eventId, Guid shiftId, Models.Salary salary, Guid authorId)
        {
            var now = DateTime.UtcNow;

            var shiftSalary = new ShiftSalary
            {
                AuthorId = authorId,
                Count = salary.Count,
                Description = salary.Description,
                ShiftId = shiftId,
                Created = now,
                ModificationDate = now
            };
            var result = await EventSalary.UpdateOneAsync(
                es => es.EventId == eventId,
                Builders<EventSalary>.Update
                    .Push(es => es.ShiftSalaries, shiftSalary)
                );
            if (result.ModifiedCount == 0)
                throw new NotFoundException();
        }
    }
}

