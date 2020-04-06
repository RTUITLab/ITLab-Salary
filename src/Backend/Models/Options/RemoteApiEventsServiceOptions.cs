using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Models.Options
{
    public class RemoteApiEventsServiceOptions
    {
        public string TokenUrl { get; set; }
        public string ClientSecret { get; set; }
        public string BaseUrl { get; set; } 
    }
}
