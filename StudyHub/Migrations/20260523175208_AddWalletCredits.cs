using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHub.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WalletBalance",
                table: "AspNetUsers",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CreditTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MomoTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    ResultCode = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PayUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_OrderId",
                table: "CreditTransactions",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_RequestId",
                table: "CreditTransactions",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_UserId",
                table: "CreditTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropColumn(
                name: "WalletBalance",
                table: "AspNetUsers");
        }
    }
}
