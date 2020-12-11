using Microsoft.EntityFrameworkCore.Migrations;

namespace FinancialPortalProject.Migrations
{
    public partial class _92 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "decimal(9, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetAmount",
                table: "CategoryItems",
                type: "decimal(9, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualAmount",
                table: "CategoryItems",
                type: "decimal(9, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "StartingBalance",
                table: "BankAccounts",
                type: "decimal(9, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LowBalance",
                table: "BankAccounts",
                type: "decimal(9, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBalance",
                table: "BankAccounts",
                type: "decimal(9, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7, 2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "decimal(7, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetAmount",
                table: "CategoryItems",
                type: "decimal(7, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualAmount",
                table: "CategoryItems",
                type: "decimal(7, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "StartingBalance",
                table: "BankAccounts",
                type: "decimal(7, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LowBalance",
                table: "BankAccounts",
                type: "decimal(7, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9, 2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBalance",
                table: "BankAccounts",
                type: "decimal(7, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9, 2)");
        }
    }
}
