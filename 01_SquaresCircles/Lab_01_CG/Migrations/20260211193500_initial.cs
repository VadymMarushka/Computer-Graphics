using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lab_01_CG.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Circles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CenterX = table.Column<double>(type: "REAL", nullable: false),
                    CenterY = table.Column<double>(type: "REAL", nullable: false),
                    Radius = table.Column<double>(type: "REAL", nullable: false),
                    OutlineColor = table.Column<string>(type: "TEXT", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Circles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Squares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CenterX = table.Column<double>(type: "REAL", nullable: false),
                    CenterY = table.Column<double>(type: "REAL", nullable: false),
                    SideLength = table.Column<double>(type: "REAL", nullable: false),
                    OutlineColor = table.Column<string>(type: "TEXT", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", nullable: false),
                    InnerCircleId = table.Column<int>(type: "INTEGER", nullable: false),
                    OuterCircleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Squares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Squares_Circles_InnerCircleId",
                        column: x => x.InnerCircleId,
                        principalTable: "Circles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Squares_Circles_OuterCircleId",
                        column: x => x.OuterCircleId,
                        principalTable: "Circles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Squares_InnerCircleId",
                table: "Squares",
                column: "InnerCircleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Squares_OuterCircleId",
                table: "Squares",
                column: "OuterCircleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Squares");

            migrationBuilder.DropTable(
                name: "Circles");
        }
    }
}
