using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarMaintenanceDiary.Infrastructure.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddUserIdToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the column as nullable first
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Vehicles",
                type: "nvarchar(450)",
                nullable: true);

            // Step 2: Set a valid UserId for existing vehicles
            // Option A: Assign all existing vehicles to the first admin user
            migrationBuilder.Sql(@"
                UPDATE v
                SET v.UserId = (
                    SELECT TOP 1 u.Id 
                    FROM AspNetUsers u
                    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                    WHERE r.Name = 'Admin'
                    ORDER BY u.Id
                )
                FROM Vehicles v
                WHERE v.UserId IS NULL
            ");

            // Step 3: If no admin exists, assign to the first user
            migrationBuilder.Sql(@"
                UPDATE v
                SET v.UserId = (
                    SELECT TOP 1 Id 
                    FROM AspNetUsers
                    ORDER BY Id
                )
                FROM Vehicles v
                WHERE v.UserId IS NULL
            ");

            // Step 4: Make the column NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Vehicles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            // Step 5: Create index
            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles",
                column: "UserId");

            // Step 6: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_UserId",
                table: "Vehicles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_UserId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Vehicles");
        }
    }
}
