using AutoMapper;
using ITLab.Salary.Models.Events;
using ITLab.Salary.Models.Reports;
using ITLab.Salary.PublicApi.Response;
using ITLab.Salary.PublicApi.Response.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITLab.Salary.Backend.Formatting
{
    /// <summary>
    /// Automapper profile for responses
    /// </summary>
    public class Responses : Profile
    {
        /// <summary>
        /// Constructor with Maps
        /// </summary>
        public Responses()
        {
            CreateMap<EventSalary, EventSalaryCompactView>();

            CreateMap<PlaceSalary, PlaceSalaryView>();
            CreateMap<ShiftSalary, ShiftSalaryView>();
            CreateMap<EventSalary, EventSalaryFullView>();

            CreateMap<ReportUserSalary, ReportUserSalaryFullView>()
                .ForMember(rusfv => rusfv.Approved, map => map.MapFrom(rus => rus.ModificationDate))
                .ForMember(rusfv => rusfv.ApproverId, map => map.MapFrom(rus => rus.AuthorId));
        }
    }
}
