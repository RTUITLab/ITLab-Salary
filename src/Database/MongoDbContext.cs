using ITLab.Salary.Models.Reports;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Database
{
    public class MongoDbContext<T>
    {
        private readonly string CollectionName = typeof(T).Name;
        private readonly string HistoryCollectionName = typeof(T).Name + "_History";

        private readonly IMongoDatabase mongoDatabase;

        public MongoDbContext(IDbFactory dbFactory)
        {
            dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            mongoDatabase = dbFactory.GetDatabase<MongoDbContext<T>>();
        }

        protected IMongoCollection<T> Collection => mongoDatabase.GetCollection<T>(CollectionName);
        private IMongoCollection<HistoryRecord<T>> History => mongoDatabase.GetCollection<HistoryRecord<T>>(HistoryCollectionName);
        protected Task SaveToHistory(T model, HistoryRecordType historyType = HistoryRecordType.Update)
        {
            return History.InsertOneAsync(new HistoryRecord<T>
            {
                SavedDate = DateTime.UtcNow,
                Object = model,
                Type = historyType
            });
        }
    }
}
