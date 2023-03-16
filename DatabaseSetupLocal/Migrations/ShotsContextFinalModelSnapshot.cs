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
    partial class ShotsContextFinalModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.3");

            modelBuilder.Entity("DatabaseSetupLocal.Models.Race", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Locked")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Points")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PolePosition")
                        .HasColumnType("TEXT");

                    b.Property<string>("RaceLocation")
                        .HasColumnType("TEXT");

                    b.Property<int>("RaceNo")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RaceYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Rand")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserShotsId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserShotsId");

                    b.ToTable("RaceModel");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.Shot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("RaceId")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("Result")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ResultDriver")
                        .HasColumnType("TEXT");

                    b.Property<string>("UsersShotDriver")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RaceId");

                    b.ToTable("ShotModel");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.UserShots", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("UserModel");
                });

            modelBuilder.Entity("DatabaseSetupLocal.Models.Race", b =>
                {
                    b.HasOne("DatabaseSetupLocal.Models.UserShots", null)
                        .WithMany("Race")
                        .HasForeignKey("UserShotsId");
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

            modelBuilder.Entity("DatabaseSetupLocal.Models.UserShots", b =>
                {
                    b.Navigation("Race");
                });
#pragma warning restore 612, 618
        }
    }
}
