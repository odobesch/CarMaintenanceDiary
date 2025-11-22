using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMaintenanceDiary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuelPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FuelRecordId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelPhotos_FuelEntries_FuelRecordId",
                        column: x => x.FuelRecordId,
                        principalTable: "FuelEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelPhotos_FuelRecordId",
                table: "FuelPhotos",
                column: "FuelRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelPhotos");
        }
    }
}
