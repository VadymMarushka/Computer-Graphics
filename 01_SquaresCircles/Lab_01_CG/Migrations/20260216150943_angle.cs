using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab_01_CG.Migrations
{
    /// <inheritdoc />
    public partial class angle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RotationAngle",
                table: "Squares",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RotationAngle",
                table: "Squares");
        }
    }
}
