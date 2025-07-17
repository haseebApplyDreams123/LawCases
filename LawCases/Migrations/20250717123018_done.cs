using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawCases.Migrations
{
    /// <inheritdoc />
    public partial class done : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_CaseDates_CaseDateId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CaseDateId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CaseDateId",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseDateId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CaseDateId",
                table: "Documents",
                column: "CaseDateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_CaseDates_CaseDateId",
                table: "Documents",
                column: "CaseDateId",
                principalTable: "CaseDates",
                principalColumn: "CaseDateId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
