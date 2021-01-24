using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameOptions",
                columns: table => new
                {
                    GameOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BoardWidth = table.Column<int>(type: "int", nullable: false),
                    BoardHeight = table.Column<int>(type: "int", nullable: false),
                    EShipsCanTouch = table.Column<int>(type: "int", nullable: false),
                    ENextMoveAfterHit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOptions", x => x.GameOptionId);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EPlayerType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    ShipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.ShipId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameOptionId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlayerAId = table.Column<int>(type: "int", nullable: false),
                    PlayerBId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_Games_GameOptions_GameOptionId",
                        column: x => x.GameOptionId,
                        principalTable: "GameOptions",
                        principalColumn: "GameOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Players_PlayerAId",
                        column: x => x.PlayerAId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Players_PlayerBId",
                        column: x => x.PlayerBId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameShips",
                columns: table => new
                {
                    GameShipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Size = table.Column<int>(type: "int", nullable: false),
                    GameShipOnBoard = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsSunken = table.Column<bool>(type: "bit", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameShips", x => x.GameShipId);
                    table.ForeignKey(
                        name: "FK_GameShips_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBoardStates",
                columns: table => new
                {
                    PlayerBoardStateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BoardSquareState = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBoardStates", x => x.PlayerBoardStateId);
                    table.ForeignKey(
                        name: "FK_PlayerBoardStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameOptionShips",
                columns: table => new
                {
                    GameOptionShipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ShipId = table.Column<int>(type: "int", nullable: false),
                    GameOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOptionShips", x => x.GameOptionShipId);
                    table.ForeignKey(
                        name: "FK_GameOptionShips_GameOptions_GameOptionId",
                        column: x => x.GameOptionId,
                        principalTable: "GameOptions",
                        principalColumn: "GameOptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameOptionShips_Ships_ShipId",
                        column: x => x.ShipId,
                        principalTable: "Ships",
                        principalColumn: "ShipId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameOptionShips_GameOptionId",
                table: "GameOptionShips",
                column: "GameOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameOptionShips_ShipId",
                table: "GameOptionShips",
                column: "ShipId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameOptionId",
                table: "Games",
                column: "GameOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_PlayerAId",
                table: "Games",
                column: "PlayerAId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_PlayerBId",
                table: "Games",
                column: "PlayerBId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameShips_PlayerId",
                table: "GameShips",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBoardStates_PlayerId",
                table: "PlayerBoardStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Ships_Name_Size",
                table: "Ships",
                columns: new[] { "Name", "Size" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameOptionShips");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "GameShips");

            migrationBuilder.DropTable(
                name: "PlayerBoardStates");

            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "GameOptions");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
