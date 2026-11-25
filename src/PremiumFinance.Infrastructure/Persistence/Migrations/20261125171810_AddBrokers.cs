using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PremiumFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrokerId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Brokers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FcaReferenceNumber = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SuspensionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CommissionDealType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CommissionRatePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ClawbackWindowMonths = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brokers", x => x.Id);
                    table.CheckConstraint("CK_Brokers_ClawbackWindow", "[ClawbackWindowMonths] BETWEEN 0 AND 12");
                    table.CheckConstraint("CK_Brokers_CommissionRate", "[CommissionRatePercent] > 0 AND [CommissionRatePercent] <= 60");
                    table.CheckConstraint("CK_Brokers_SuspensionReason", "[Status] <> 'Suspended' OR [SuspensionReason] IS NOT NULL");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BrokerId",
                table: "AspNetUsers",
                column: "BrokerId");

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_FcaReferenceNumber",
                table: "Brokers",
                column: "FcaReferenceNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Brokers_BrokerId",
                table: "AspNetUsers",
                column: "BrokerId",
                principalTable: "Brokers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Brokers_BrokerId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Brokers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BrokerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrokerId",
                table: "AspNetUsers");
        }
    }
}
