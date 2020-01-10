using AutoMapper;
using ITLab.Salary.Models;
using ITLab.Salary.PublicApi.Request.Salary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Formatting
{
    public class Requests : Profile
    {
        public Requests()
        {

            CreateMap<SalaryInfo, Models.Salary>();

            CreateMap<PlaceSalaryCreate, PlaceSalary>();
            CreateMap<ShiftSalaryCreate, ShiftSalary>();
            CreateMap<EventSalaryCreate, EventSalary>();
        }
    }
}
