using Microsoft.EntityFrameworkCore.Migrations;

namespace PartyBot.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: true),
                    TotalTimesPlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    Artist = table.Column<string>(type: "TEXT", nullable: true),
                    TimesCorrect = table.Column<int>(type: "INTEGER", nullable: false),
                    FromList = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Show = table.Column<string>(type: "TEXT", nullable: true),
                    SongName = table.Column<string>(type: "TEXT", nullable: true),
                    Romaji = table.Column<string>(type: "TEXT", nullable: true),
                    Rule = table.Column<string>(type: "TEXT", nullable: true),
                    AnnID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SongTableObject",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    SongName = table.Column<string>(type: "TEXT", nullable: true),
                    Artist = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Show = table.Column<string>(type: "TEXT", nullable: true),
                    Romaji = table.Column<string>(type: "TEXT", nullable: true),
                    MP3 = table.Column<string>(type: "TEXT", nullable: true),
                    _720 = table.Column<string>(type: "TEXT", nullable: true),
                    _480 = table.Column<string>(type: "TEXT", nullable: true),
                    AnnID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongTableObject", x => x.Key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "SongTableObject");
        }
    }
}
