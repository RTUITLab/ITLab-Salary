using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class MetaSalary : Salary
    {
        public DateTime Created { get; set; }
        public DateTime ModificationDate { get; set; }
        public Guid AuthorId { get; set; }
    }
}
