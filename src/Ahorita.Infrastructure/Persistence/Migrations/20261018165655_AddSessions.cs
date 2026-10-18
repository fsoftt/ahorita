using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ahorita.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoutineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoutineName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    RoutineEmoji = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RoutineAccent = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    StartedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    FinishedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CurrentStepStartedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    PausedAt = table.Column<long>(type: "INTEGER", nullable: true),
                    CurrentStepPausedTotal = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceStepId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    PlannedDuration = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Outcome = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    WasPostponed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActualDuration = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionSteps_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RoutineId",
                table: "Sessions",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StartedAt",
                table: "Sessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Status",
                table: "Sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSteps_SessionId",
                table: "SessionSteps",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionSteps");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
