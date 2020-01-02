﻿// <auto-generated />
using System;
using ITLab.Salary.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ITLab.Salary.Backend.Migrations
{
    [DbContext(typeof(SalaryDbContext))]
    partial class SalaryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ITLab.Salary.Models.EventSalary", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("EventId")
                        .IsUnique();

                    b.ToTable("EventSalaries");

                    b.HasDiscriminator<string>("Discriminator").HasValue("EventSalary");
                });

            modelBuilder.Entity("ITLab.Salary.Models.ShiftSalary", b =>
                {
                    b.HasBaseType("ITLab.Salary.Models.EventSalary");

                    b.Property<Guid>("ShiftId")
                        .HasColumnType("uuid");

                    b.HasDiscriminator().HasValue("ShiftSalary");
                });

            modelBuilder.Entity("ITLab.Salary.Models.PlaceSalary", b =>
                {
                    b.HasBaseType("ITLab.Salary.Models.ShiftSalary");

                    b.Property<Guid>("PlaceId")
                        .HasColumnType("uuid");

                    b.HasDiscriminator().HasValue("PlaceSalary");
                });

            modelBuilder.Entity("ITLab.Salary.Models.UserSalary", b =>
                {
                    b.HasBaseType("ITLab.Salary.Models.PlaceSalary");

                    b.Property<DateTime>("Approved")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("ApproverId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasDiscriminator().HasValue("UserSalary");
                });
#pragma warning restore 612, 618
        }
    }
}
