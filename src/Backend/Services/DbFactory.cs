using ITLab.Salary.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Services
{
    /// <summary>
    /// Factory based on typeDictionary
    /// </summary>
    public class ConcurrentDictionaryDbFactory : IDbFactory
    {
        private readonly ILogger<ConcurrentDictionaryDbFactory> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly MongoUrl mongoUrl;

        private readonly ConcurrentDictionary<Type, IMongoDatabase> createdDatabases = new ConcurrentDictionary<Type, IMongoDatabase>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration">Configuration for connection string</param>
        /// <param name="logger">Logeer for <see cref="ConcurrentDictionaryDbFactory"></see></param>
        /// <param name="loggerFactory">Factory for creating custom logger for each type</param>
        public ConcurrentDictionaryDbFactory(
            IConfiguration configuration,
            ILogger<ConcurrentDictionaryDbFactory> logger,
            ILoggerFactory loggerFactory)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
            mongoUrl = new MongoUrl(connectionString);
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }
        /// <summary>
        /// Create or returns database for type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IMongoDatabase GetDatabase<T>()
        {
            return createdDatabases.GetOrAdd(typeof(T), CreateDatabase);
        }

        private IMongoDatabase CreateDatabase(Type targetType)
        {
            logger.LogDebug($"Creating mongo client for type {targetType.Name}");
            var typeLogger = loggerFactory.CreateLogger(targetType);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoUrl);
            mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    typeLogger.LogDebug($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };
            MongoClient client = new MongoClient(mongoClientSettings);
            return client.GetDatabase(mongoUrl.DatabaseName);
        }
    }
}
