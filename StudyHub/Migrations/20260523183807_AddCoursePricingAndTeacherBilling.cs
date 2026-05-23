using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyHub.Migrations
{
    /// <inheritdoc />
    public partial class AddCoursePricingAndTeacherBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePaid",
                table: "Enrollments",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "decimal(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "IsTeacherSuspended",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTeacherBillingNoticeAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextTeacherBillingAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeacherBillingStartsAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeacherSuspendedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherSuspensionReason",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePaid",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsTeacherSuspended",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastTeacherBillingNoticeAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NextTeacherBillingAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeacherBillingStartsAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeacherSuspendedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeacherSuspensionReason",
                table: "AspNetUsers");
        }
    }
}
