using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Core.Events;
using ITLab.Salary.Models;
using System.Threading.Tasks;
using System.Linq.Expressions;
using ITLab.Salary.Shared.Exceptions;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ITLab.Salary.Database
{
    public class EventSalaryContext
    {
        public const string EventSalaryCollectionName = nameof(EventSalary);
        public const string EventSalaryHistoryCollectionName = nameof(EventSalary) + "_History";

        private readonly IMongoDatabase mongoDatabase;
        private readonly ILogger<EventSalaryContext> logger;

        public EventSalaryContext(IDbFactory dbFactory, ILogger<EventSalaryContext> logger)
        {
            dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            mongoDatabase = dbFactory.GetDatabase<EventSalaryContext>();
            this.logger = logger;
        }

        private IMongoCollection<EventSalary> Collection => mongoDatabase.GetCollection<EventSalary>(EventSalaryCollectionName);
        private IMongoCollection<HistoryRecord<EventSalary>> History => mongoDatabase.GetCollection<HistoryRecord<EventSalary>>(EventSalaryHistoryCollectionName);
        private Task SaveToHistory(EventSalary eventSalary, HistoryRecordType historyType = HistoryRecordType.Update)
        {
            return History.InsertOneAsync(new HistoryRecord<EventSalary>
            {
                SavedDate = DateTime.UtcNow,
                Object = eventSalary,
                Type = historyType
            });
        }


        public async Task<List<EventSalary>> GetAll()
        {
            return (await Collection.Find(Builders<EventSalary>.Filter.Empty).ToListAsync().ConfigureAwait(false))
                    .Select(Clean)
                    .ToList();
        }

        public Task<List<T>> GetAll<T>(Expression<Func<EventSalary, T>> projection)
        {
            return Collection.Find(Builders<EventSalary>.Filter.Empty).Project(projection).ToListAsync();
        }

        public async Task<EventSalary> GetOneOrDefault(Guid eventId)
        {
            return (Clean(await Collection.Find(es => es.EventId == eventId).SingleOrDefaultAsync().ConfigureAwait(false)));
        }

        public Task<T> GetOneOrDefault<T>(Guid eventId, Expression<Func<EventSalary, T>> projection)
        {
            return Collection.Find(es => es.EventId == eventId).Project(projection).SingleOrDefaultAsync();
        }

        public async Task<EventSalary> UpdateEvenInfo(Guid eventId, EventSalary eventSalary, Guid authorId)
        {
            eventSalary = eventSalary ?? throw new ArgumentNullException(nameof(eventSalary));
            var now = DateTime.UtcNow;
            eventSalary.EventId = eventId;
            eventSalary.Created = eventSalary.ModificationDate = now;
            eventSalary.AuthorId = authorId;
            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId),
                Builders<EventSalary>.Update
                    .Set(es => es.EventId, eventSalary.EventId)
                    .SetOnInsert(es => es.Created, eventSalary.Created)
                    .Set(es => es.ModificationDate, eventSalary.ModificationDate)
                    .Set(es => es.Count, eventSalary.Count)
                    .Set(es => es.AuthorId, eventSalary.AuthorId)
                    .Set(es => es.Description, eventSalary.Description)
                    .Set(es => es.ShiftSalaries, eventSalary.ShiftSalaries)
                    .Set(es => es.PlaceSalaries, eventSalary.PlaceSalaries),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
                ).ConfigureAwait(false);
            await SaveToHistory(updated).ConfigureAwait(false);
            return Clean(updated);
        }

        public async Task Delete(Guid eventId, Guid userId)
        {
            var deleted = await Collection.FindOneAndDeleteAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId)).ConfigureAwait(false);
            if (deleted == null)
                throw new NotFoundException("Can't delete event salary");
            deleted.ModificationDate = DateTime.UtcNow;
            deleted.AuthorId = userId;
            await SaveToHistory(deleted, HistoryRecordType.Delete).ConfigureAwait(false);
        }

        private EventSalary Clean(EventSalary eventSalary)
        {
            if (eventSalary == null)
                return eventSalary;
            var set = new HashSet<Guid>();
            foreach (var shiftSalary in eventSalary.ShiftSalaries?.ToArray() ?? Enumerable.Empty<ShiftSalary>())
            {
                if (!set.Add(shiftSalary.ShiftId))
                    eventSalary.ShiftSalaries.Remove(shiftSalary);
            }
            set.Clear();
            foreach (var placeSalary in eventSalary.PlaceSalaries?.ToArray() ?? Enumerable.Empty<PlaceSalary>())
            {
                if (!set.Add(placeSalary.PlaceId))
                    eventSalary.PlaceSalaries.Remove(placeSalary);
            }
            return eventSalary;
        }
    }
}
