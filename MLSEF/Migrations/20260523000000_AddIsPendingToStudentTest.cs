using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MLSEF.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPendingToStudentTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "StudentTests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "StudentTests");
        }
    }
}
