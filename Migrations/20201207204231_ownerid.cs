using Microsoft.EntityFrameworkCore.Migrations;

namespace FinancialPortalProject.Migrations
{
    public partial class ownerid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_AspNetUsers_FpUserId",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_FpUserId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "FpUserId",
                table: "BankAccounts");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "BankAccounts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_OwnerId",
                table: "BankAccounts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_AspNetUsers_OwnerId",
                table: "BankAccounts",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_AspNetUsers_OwnerId",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_OwnerId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "BankAccounts");

            migrationBuilder.AddColumn<string>(
                name: "FpUserId",
                table: "BankAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_FpUserId",
                table: "BankAccounts",
                column: "FpUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_AspNetUsers_FpUserId",
                table: "BankAccounts",
                column: "FpUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
