using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PartyBot.DataStructs;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PartyBot.Database
{
    public partial class AMQDBContext : DbContext
    {
        private readonly char separator = Path.DirectorySeparatorChar;
        public virtual DbSet<SongTableObject> SongTableObject { get; set; }
        public virtual DbSet<PlayerTableObject> PlayerStats { get; set; }
        public virtual DbSet<CircuitTeamTableObject> CircuitTeams { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string mainpath = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                mainpath = mainpath.Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
                mainpath = separator + mainpath;
            }
            else
            {
                mainpath = mainpath.Replace($"{separator}bin{separator}debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            }
            Console.WriteLine(mainpath);
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource =
                Path.Combine(mainpath, "Database", "MugiBotDatbase.db")
            };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            optionsBuilder.UseSqlite(connection);
        }
    }
}
