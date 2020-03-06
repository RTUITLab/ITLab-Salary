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

        public async Task<EventSalary> AddNew(Guid eventId, EventSalary eventSalary, Guid authorId)
        {
            eventSalary = eventSalary ?? throw new ArgumentNullException(nameof(eventSalary));
            var now = DateTime.UtcNow;
            eventSalary.EventId = eventId;
            eventSalary.Created = eventSalary.ModificationDate = now;
            eventSalary.AuthorId = authorId;
            eventSalary?.ShiftSalaries.ForEach(ss =>
            {
                ss.ModificationDate = now;
                ss.AuthorId = authorId;

            });
            eventSalary?.PlaceSalaries.ForEach(ps =>
            {
                ps.ModificationDate = now;
                ps.AuthorId = authorId;
            });

            await Collection.InsertOneAsync(eventSalary).ConfigureAwait(false);
            await SaveToHistory(eventSalary).ConfigureAwait(false);
            return Clean(eventSalary);
        }

        public async Task<EventSalary> UpdateEvenInfo(Guid eventId, SalaryModel info, Guid authorId)
        {
            info = info ?? throw new ArgumentNullException(nameof(info));

            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId),
                Builders<EventSalary>.Update
                    .Set(es => es.ModificationDate, DateTime.UtcNow)
                    .Set(es => es.AuthorId, authorId)
                    .Set(es => es.Description, info.Description)
                    .Set(es => es.Count, info.Count),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }
                ).ConfigureAwait(false);
            if (updated == null)
                throw new NotFoundException("Not found event salary");
            await SaveToHistory(updated).ConfigureAwait(false);
            return Clean(updated);
        }

        public async Task<EventSalary> UpdateShiftInfo(Guid eventId, Guid shiftId, Models.SalaryModel info, Guid authorId)
        {
            info = info ?? throw new ArgumentNullException(nameof(info));
            try
            {
                var now = DateTime.UtcNow;
                // Add new shift information
                var updated = await Collection.FindOneAndUpdateAsync(
                    Builders<EventSalary>.Filter.Where(es => es.EventId == eventId),
                    Builders<EventSalary>.Update
                        .Push(es => es.ShiftSalaries, new ShiftSalary
                        {
                            ModificationDate = now,
                            AuthorId = authorId,
                            Description = info.Description,
                            Count = info.Count,
                            ShiftId = shiftId
                        }),
                    new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }).ConfigureAwait(false);
                if (updated == null)
                    throw new NotFoundException("Event salary not found");
                await SaveToHistory(updated).ConfigureAwait(false);

                // Remove old information
                updated = await Collection.FindOneAndUpdateAsync(
                    Builders<EventSalary>.Filter.Where(es => es.EventId == eventId),
                    Builders<EventSalary>.Update
                        .PullFilter(
                            es => es.ShiftSalaries,
                            ss => ss.ShiftId == shiftId && ss.ModificationDate < now),
                    new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }).ConfigureAwait(false);
                if (updated == null)
                    throw new NotFoundException("Event salary not found");
                await SaveToHistory(updated).ConfigureAwait(false);

                return Clean(updated);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<EventSalary> UpdatePlaceInfo(
            Guid eventId,
            Guid placeId,
            Models.SalaryModel info,
            Guid authorId)
        {
            info = info ?? throw new ArgumentNullException(nameof(info));
            var now = DateTime.UtcNow;
            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId),
                Builders<EventSalary>.Update
                    .Push(es => es.PlaceSalaries, new PlaceSalary
                    {
                        PlaceId = placeId,
                        AuthorId = authorId,
                        Count = info.Count,
                        Description = info.Description,
                        ModificationDate = now
                    }),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }).ConfigureAwait(false);
            if (updated == null)
                throw new NotFoundException("Event salary not found");
            await SaveToHistory(updated).ConfigureAwait(false);

            updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId),
                Builders<EventSalary>.Update
                    .PullFilter(es => es.PlaceSalaries, 
                        ps => ps.PlaceId == placeId && ps.ModificationDate < now),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }).ConfigureAwait(false);
            if (updated == null)
                throw new NotFoundException("Event salary not found");
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
            var set = new HashSet<Guid>();
            foreach (var shiftSalary in eventSalary.ShiftSalaries.ToArray())
            {
                if (!set.Add(shiftSalary.ShiftId))
                    eventSalary.ShiftSalaries.Remove(shiftSalary);
            }
            set.Clear();
            foreach (var placeSalary in eventSalary.PlaceSalaries.ToArray())
            {
                if (!set.Add(placeSalary.PlaceId))
                    eventSalary.PlaceSalaries.Remove(placeSalary);
            }
            return eventSalary;
        }
    }
}
