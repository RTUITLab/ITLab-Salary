using ITLab.Salary.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Database
{
    public class HistoryRecord<T>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime SavedDate { get; set; }
        public T Object { get; set; }
    }
}
