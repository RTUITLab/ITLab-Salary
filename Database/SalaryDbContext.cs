using ITLab.Salary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Database
{
    public class SalaryDbContext : DbContext
    {
        public SalaryDbContext(DbContextOptions<SalaryDbContext> options) : base(options)
        { }

        public DbSet<EventSalary>  EventSalaries { get; set; }
        public DbSet<ShiftSalary> ShiftSalaries { get; set; }
        public DbSet<PlaceSalary> PlaceSalaries { get; set; }
        public DbSet<UserSalary> UserSalaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventSalary>(builder =>
            {
                builder.HasIndex(es => new { es.EventId }).IsUnique();
            });
        }
    }
}
