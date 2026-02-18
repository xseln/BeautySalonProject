using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautySalonProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeWorkHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeWorkHours");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeWorkHours",
                columns: table => new
                {
                    EmployeeWorkHourId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsWorking = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkHours", x => x.EmployeeWorkHourId);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkHours_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkHours_EmployeeId_DayOfWeek",
                table: "EmployeeWorkHours",
                columns: new[] { "EmployeeId", "DayOfWeek" },
                unique: true);
        }
    }
}
