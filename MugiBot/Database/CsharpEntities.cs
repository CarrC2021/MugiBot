using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PartyBot.DataStructs;
using PartyBot.Handlers;
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
            string mainpath = GlobalData.Config.RootFolderPath;
            Console.WriteLine(mainpath);
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource =
                Path.Combine(mainpath, "Database", "MugiBotDatbase.db")
            };
            var connectionString = connectionStringBuilder.ToString();
            Console.WriteLine(connectionString);
            var connection = new SqliteConnection(connectionString);
            optionsBuilder.UseSqlite(connection);
        }
    }
}
