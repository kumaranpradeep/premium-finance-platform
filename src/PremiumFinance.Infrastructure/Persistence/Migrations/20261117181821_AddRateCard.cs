using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PremiumFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRateCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RateCardEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumberOfInstalments = table.Column<int>(type: "int", nullable: false),
                    AnnualFlatRatePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateCardEntries", x => x.Id);
                    table.CheckConstraint("CK_RateCardEntries_Dates", "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
                    table.CheckConstraint("CK_RateCardEntries_Instalments", "[NumberOfInstalments] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RateCardEntries_Rate", "[AnnualFlatRatePercent] > 0 AND [AnnualFlatRatePercent] <= 50");
                });

            migrationBuilder.InsertData(
                table: "RateCardEntries",
                columns: new[] { "Id", "AnnualFlatRatePercent", "CreatedBy", "CreatedOnUtc", "EffectiveFrom", "EffectiveTo", "ModifiedBy", "ModifiedOnUtc", "NumberOfInstalments", "SchemeCode" },
                values: new object[,]
                {
                    { 1, 9.50m, "migration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 1, 1), null, null, null, 10, "STANDARD" },
                    { 2, 9.75m, "migration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 1, 1), null, null, null, 11, "STANDARD" },
                    { 3, 9.90m, "migration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null, null, 12, "STANDARD" },
                    { 4, 10.50m, "migration", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2027, 1, 1), null, null, null, 12, "STANDARD" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RateCardEntries_SchemeCode_NumberOfInstalments_EffectiveFrom",
                table: "RateCardEntries",
                columns: new[] { "SchemeCode", "NumberOfInstalments", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateCardEntries");
        }
    }
}
