﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PartyBot.Database;

namespace PartyBot.Migrations
{
    [DbContext(typeof(AMQDBContext))]
    [Migration("20210804221224_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("PartyBot.Database.PlayerTableObject", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<int>("AnnID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .HasColumnType("TEXT");

                    b.Property<int>("FromList")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Romaji")
                        .HasColumnType("TEXT");

                    b.Property<string>("Rule")
                        .HasColumnType("TEXT");

                    b.Property<string>("Show")
                        .HasColumnType("TEXT");

                    b.Property<string>("SongName")
                        .HasColumnType("TEXT");

                    b.Property<int>("TimesCorrect")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TotalTimesPlayed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("PlayerStats");
                });

            modelBuilder.Entity("PartyBot.Database.SongTableObject", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<int>("AnnID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Artist")
                        .HasColumnType("TEXT");

                    b.Property<string>("MP3")
                        .HasColumnType("TEXT");

                    b.Property<string>("Romaji")
                        .HasColumnType("TEXT");

                    b.Property<string>("Show")
                        .HasColumnType("TEXT");

                    b.Property<string>("SongName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.Property<string>("_480")
                        .HasColumnType("TEXT");

                    b.Property<string>("_720")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("SongTableObject");
                });
#pragma warning restore 612, 618
        }
    }
}
