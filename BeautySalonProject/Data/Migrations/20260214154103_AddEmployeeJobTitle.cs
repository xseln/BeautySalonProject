using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautySalonProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeJobTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Employees");
        }
    }
}
