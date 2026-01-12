using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingPlayground.Migrations
{
    /// <inheritdoc />
    public partial class addTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Fin_FinancialAccounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsLeaf",
                table: "Fin_FinancialAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Fin_FinancialAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Fin_FinancialAccounts_Code",
                table: "Fin_FinancialAccounts",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fin_FinancialAccounts_Code",
                table: "Fin_FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Fin_FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "IsLeaf",
                table: "Fin_FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Fin_FinancialAccounts");
        }
    }
}
