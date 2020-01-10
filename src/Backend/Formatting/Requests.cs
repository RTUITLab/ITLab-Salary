using AutoMapper;
using ITLab.Salary.Models;
using ITLab.Salary.PublicApi.Request.Salary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Formatting
{
    /// <summary>
    /// Automapper profile for requests
    /// </summary>
    public class Requests : Profile
    {
        /// <summary>
        /// Constructor with Maps
        /// </summary>
        public Requests()
        {

            CreateMap<SalaryInfo, Models.Salary>();

            CreateMap<PlaceSalaryCreate, PlaceSalary>();
            CreateMap<ShiftSalaryCreate, ShiftSalary>();
            CreateMap<EventSalaryCreate, EventSalary>();
        }
    }
}
