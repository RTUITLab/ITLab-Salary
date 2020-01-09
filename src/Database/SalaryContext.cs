using ITLab.Salary.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITLab.Salary.Database
{
    public class SalaryContext
    {
        public IMongoDatabase Database { get; }
        public SalaryContext(string connectionString)
        {
            var connection = new MongoUrlBuilder(connectionString);
            MongoClient client = new MongoClient(connectionString);
            Database = client.GetDatabase(connection.DatabaseName);
        }

        public IMongoCollection<EventSalary> EventSalary => Database.GetCollection<EventSalary>(nameof(EventSalary));
    }
}
