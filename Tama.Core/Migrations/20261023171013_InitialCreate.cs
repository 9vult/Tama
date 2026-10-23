using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tama.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brackets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    AllowTie = table.Column<bool>(type: "INTEGER", nullable: false),
                    EntryLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxConcurrentMatches = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowOpenings = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowEndings = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowInserts = table.Column<bool>(type: "INTEGER", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brackets", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BracketId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubmitterId = table.Column<string>(type: "TEXT", nullable: false),
                    Seed = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SubmissionUrl = table.Column<string>(
                        type: "TEXT",
                        maxLength: 256,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_Brackets_BracketId",
                        column: x => x.BracketId,
                        principalTable: "Brackets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BracketId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Round = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    NextId = table.Column<Guid>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    LeftId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RightId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WinnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Brackets_BracketId",
                        column: x => x.BracketId,
                        principalTable: "Brackets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Matches_Entries_LeftId",
                        column: x => x.LeftId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Matches_Entries_RightId",
                        column: x => x.RightId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Matches_Entries_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Entries_BracketId",
                table: "Entries",
                column: "BracketId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_BracketId_Round_Index",
                table: "Matches",
                columns: new[] { "BracketId", "Round", "Index" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LeftId",
                table: "Matches",
                column: "LeftId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RightId",
                table: "Matches",
                column: "RightId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_WinnerId",
                table: "Matches",
                column: "WinnerId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Matches");

            migrationBuilder.DropTable(name: "Entries");

            migrationBuilder.DropTable(name: "Brackets");
        }
    }
}
