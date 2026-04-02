using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab_03_CG.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fractals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    FractalType = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Formula = table.Column<string>(type: "TEXT", nullable: true),
                    Family = table.Column<string>(type: "TEXT", nullable: true),
                    Palette = table.Column<string>(type: "TEXT", nullable: true),
                    Z0_Real = table.Column<double>(type: "REAL", nullable: true),
                    Z0_Imaginary = table.Column<double>(type: "REAL", nullable: true),
                    C_Real = table.Column<double>(type: "REAL", nullable: true),
                    C_Imaginary = table.Column<double>(type: "REAL", nullable: true),
                    EscapeRadius = table.Column<double>(type: "REAL", nullable: true),
                    MaxIterations = table.Column<int>(type: "INTEGER", nullable: true),
                    Initializer = table.Column<string>(type: "TEXT", nullable: true),
                    Generator = table.Column<string>(type: "TEXT", nullable: true),
                    CenterX = table.Column<double>(type: "REAL", nullable: true),
                    CenterY = table.Column<double>(type: "REAL", nullable: true),
                    SideLength = table.Column<double>(type: "REAL", nullable: true),
                    RotationAngle = table.Column<int>(type: "INTEGER", nullable: true),
                    LineColor = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fractals", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fractals");
        }
    }
}
