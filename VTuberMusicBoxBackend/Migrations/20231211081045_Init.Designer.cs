﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VTuberMusicBoxBackend.Models.Database;

#nullable disable

namespace VTuberMusicBoxBackend.Migrations
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20231211081045_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("VTuberMusicBoxBackend.Models.Database.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateAdded")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("DiscordUserId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<ushort>("Position")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("VideoIdList")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("VTuberMusicBoxBackend.Models.Database.Track", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("DateAdded")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("DiscordUserId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<ushort>("EndAt")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("Guid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<ushort>("StartAt")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("TrackTitle")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("Unplayable")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("VideoId")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("VideoTitle")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Track");
                });
#pragma warning restore 612, 618
        }
    }
}