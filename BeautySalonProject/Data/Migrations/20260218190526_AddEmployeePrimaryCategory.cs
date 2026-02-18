using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautySalonProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePrimaryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrimaryCategoryId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PrimaryCategoryId",
                table: "Employees",
                column: "PrimaryCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ServiceCategories_PrimaryCategoryId",
                table: "Employees",
                column: "PrimaryCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ServiceCategories_PrimaryCategoryId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PrimaryCategoryId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PrimaryCategoryId",
                table: "Employees");
        }
    }
}
