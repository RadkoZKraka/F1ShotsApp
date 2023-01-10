﻿// <auto-generated />
using System;
using DatabaseSetupLocal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DatabaseSetupLocal.Migrations
{
    [DbContext(typeof(ShotsContext))]
    partial class ShotsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("DatabaseSetupLocal.Models.Race", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("PolePosition")
                        .HasColumnType("longtext");

                    b.Property<string>("RaceLocation")
                        .HasColumnType("longtext");

                    b.Property<int>("RaceNo")
                        .HasColumnType("int");

                    b.Property<int>("RaceYear")
                        .HasColumnType("int");

                    b.Property<string>("Rand")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RaceModel");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.Shot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Driver")
                        .HasColumnType("longtext");

                    b.Property<int?>("RaceId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RaceId");

                    b.ToTable("ShotModel");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("UserModel");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.Race", b =>
                {
                    b.HasOne("DatabaseSetupLocal.Models.User", null)
                        .WithMany("Race")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.Shot", b =>
                {
                    b.HasOne("DatabaseSetupLocal.Models.Race", null)
                        .WithMany("Shot")
                        .HasForeignKey("RaceId");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.Race", b =>
                {
                    b.Navigation("Shot");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.User", b =>
                {
                    b.Navigation("Race");
                });
#pragma warning restore 612, 618
        }
    }
}
