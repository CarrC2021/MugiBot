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
        public virtual DbSet<DiscordUser> DiscordUsers { get; set; }
        public virtual DbSet<AnimeRelationalMap> AnimeRelationalMaps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string mainpath = GlobalData.Config.RootFolderPath;
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource =
                Path.Combine(mainpath, "Database", "MugiBotDatabase.db")
            };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            optionsBuilder.UseSqlite(connection);
        }
    }
}
