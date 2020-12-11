using Microsoft.EntityFrameworkCore.Migrations;

namespace FinancialPortalProject.Migrations
{
    public partial class lowBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LowBalance",
                table: "BankAccounts",
                type: "decimal(7, 2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowBalance",
                table: "BankAccounts");
        }
    }
}
