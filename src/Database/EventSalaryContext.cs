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
        private Task SaveToHistory(EventSalary eventSalary)
        {
            return History.InsertOneAsync(new HistoryRecord<EventSalary>
            {
                SavedDate = DateTime.UtcNow,
                Object = eventSalary
            });
        }


        public Task<List<EventSalary>> GetAll()
        {
            return Collection.Find(Builders<EventSalary>.Filter.Empty).ToListAsync();
        }

        public Task<List<T>> GetAll<T>(Expression<Func<EventSalary, T>> projection)
        {
            return Collection.Find(Builders<EventSalary>.Filter.Empty).Project(projection).ToListAsync();
        }

        public Task<EventSalary> GetOneOrDefault(Guid eventId)
        {
            return Collection.Find(es => es.EventId == eventId).SingleOrDefaultAsync();
        }

        public Task<T> GetOneOrDefault<T>(Guid eventId, Expression<Func<EventSalary, T>> projection)
        {
            return Collection.Find(es => es.EventId == eventId).Project(projection).SingleOrDefaultAsync();
        }

        public async Task AddNew(Guid eventId, EventSalary eventSalary, Guid authorId)
        {
            eventSalary = eventSalary ?? throw new ArgumentNullException(nameof(eventSalary));
            var now = DateTime.UtcNow;
            eventSalary.EventId = eventId;
            eventSalary.Created = eventSalary.ModificationDate = now;
            eventSalary.AuthorId = authorId;
            eventSalary?.ShiftSalaries.ForEach(ss =>
            {
                ss.Created = ss.ModificationDate = now;
                ss.AuthorId = authorId;
                
            });
            eventSalary?.PlaceSalaries.ForEach(ps =>
            {
                ps.Created = ps.ModificationDate = now;
                ps.AuthorId = authorId;
            });

            await Collection.InsertOneAsync(eventSalary).ConfigureAwait(false);
            await SaveToHistory(eventSalary).ConfigureAwait(false);
        }

        public async Task<EventSalary> UpdateEvenInfo(Guid eventId, Models.Salary info, Guid authorId)
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
            return updated;
        }


        public async Task<EventSalary> AddShift(Guid eventId, Guid shiftId, Models.Salary salary, Guid authorId)
        {
            salary = salary ?? throw new ArgumentNullException(nameof(salary));
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
            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.And(
                    Builders<EventSalary>.Filter.Eq(es => es.EventId, eventId),
                    Builders<EventSalary>.Filter.Ne($"{nameof(Models.EventSalary.ShiftSalaries)}.{nameof(Models.ShiftSalary.ShiftId)}", shiftId)
                ),
                Builders<EventSalary>.Update
                    .Push(es => es.ShiftSalaries, shiftSalary),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }
                ).ConfigureAwait(false);
            if (updated == null)
            {
                var targetEventSalary = await GetOneOrDefault(eventId, es => new { es.EventId, ssids = es.ShiftSalaries.Select(ss => ss.ShiftId) }).ConfigureAwait(false);
                if (targetEventSalary == null)
                    throw new NotFoundException("Not found event salary");
                if (targetEventSalary.ssids.Contains(shiftId))
                    throw new BadRequestException("Shift salary in event salary already exists");
                throw new Exception("Error while shift salary adding");
            }
            await SaveToHistory(updated).ConfigureAwait(false);
            return updated;
        }

        public async Task<EventSalary> UpdateShiftInfo(Guid eventId, Guid shiftId, Models.Salary info, Guid authorId)
        {
            info = info ?? throw new ArgumentNullException(nameof(info));
            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId && es.ShiftSalaries.Any(ss => ss.ShiftId == shiftId)),
                Builders<EventSalary>.Update
                    .Set(es => es.ShiftSalaries[-1].ModificationDate, DateTime.UtcNow)
                    .Set(es => es.ShiftSalaries[-1].AuthorId, authorId)
                    .Set(es => es.ShiftSalaries[-1].Description, info.Description)
                    .Set(es => es.ShiftSalaries[-1].Count, info.Count),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }).ConfigureAwait(false);
            if (updated == null)
                throw new NotFoundException("Event or shift salary not found");
            await SaveToHistory(updated).ConfigureAwait(false);
            return updated;
        }

        public async Task<EventSalary> AddPlace(
            Guid eventId,
            Guid placeId,
            Models.Salary info,
            Guid authorId)
        {
            info = info ?? throw new ArgumentNullException(nameof(info));

            var now = DateTime.UtcNow;

            var placeSalary = new PlaceSalary
            {
                AuthorId = authorId,
                Count = info.Count,
                Description = info.Description,
                PlaceId = placeId,
                Created = now,
                ModificationDate = now
            };

            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.And(
                    Builders<EventSalary>.Filter.Eq(es => es.EventId, eventId),
                    Builders<EventSalary>.Filter.Ne($"{nameof(Models.EventSalary.PlaceSalaries)}.{nameof(Models.PlaceSalary.PlaceId)}", placeId)
                ),
                Builders<EventSalary>.Update
                    .Push(es => es.PlaceSalaries, placeSalary),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }
                ).ConfigureAwait(false);
            if (updated == null)
            {
                var targetEventSalary = await GetOneOrDefault(eventId, es => new { es.EventId, psids = es.PlaceSalaries.Select(ss => ss.PlaceId) }).ConfigureAwait(false);

                if (targetEventSalary == null)
                    throw new NotFoundException("Not found event salary");
                if (targetEventSalary.psids.Contains(placeId))
                    throw new BadRequestException("Place salary in event salary already exists");
                throw new Exception("Error while place salary adding");
            }
            await SaveToHistory(updated).ConfigureAwait(false);
            return updated;
        }

        public async Task<EventSalary> UpdatePlaceInfo(
            Guid eventId,
            Guid placeId,
            Models.Salary info,
            Guid authorId)
        {
            info = info ?? throw new ArgumentNullException(nameof(info));
            var updated = await Collection.FindOneAndUpdateAsync(
                Builders<EventSalary>.Filter.Where(es => es.EventId == eventId && es.PlaceSalaries.Any(ps => ps.PlaceId == placeId)),
                Builders<EventSalary>.Update
                    .Set(es => es.PlaceSalaries[-1].ModificationDate, DateTime.UtcNow)
                    .Set(es => es.PlaceSalaries[-1].AuthorId, authorId)
                    .Set(es => es.PlaceSalaries[-1].Description, info.Description)
                    .Set(es => es.PlaceSalaries[-1].Count, info.Count),
                new FindOneAndUpdateOptions<EventSalary, EventSalary> { ReturnDocument = ReturnDocument.After }).ConfigureAwait(false);
            if (updated == null)
                throw new NotFoundException("Event or place salary not found");
            await SaveToHistory(updated).ConfigureAwait(false);
            return updated;
        }
    }
}
