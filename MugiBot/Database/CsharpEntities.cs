using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mainpath = separator + mainpath;
            Console.WriteLine(mainpath);
            //when first building the database you have to hard code the path, it really does not like 
            //the substring function
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
