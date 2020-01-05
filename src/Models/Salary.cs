using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Models
{
    public class Salary
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
        public Guid AuthorId { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
    }
}
