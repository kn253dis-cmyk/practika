using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banking_system.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCreditLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccruedInterest",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "LastReminderSentDate",
                table: "Cards",
                newName: "TermEndDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsBlacklisted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InterestAppliedCount",
                table: "Cards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWarningSentDate",
                table: "Cards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanDurationMonths",
                table: "Cards",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBlacklisted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InterestAppliedCount",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "LastWarningSentDate",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "PlanDurationMonths",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "TermEndDate",
                table: "Cards",
                newName: "LastReminderSentDate");

            migrationBuilder.AddColumn<decimal>(
                name: "AccruedInterest",
                table: "Cards",
                type: "numeric",
                nullable: true);
        }
    }
}
