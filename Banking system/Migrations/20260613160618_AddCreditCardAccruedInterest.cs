using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banking_system.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardAccruedInterest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "Cards",
                newName: "InterestRate");

            migrationBuilder.RenameColumn(
                name: "CreditEndDate",
                table: "Cards",
                newName: "LastReminderSentDate");

            migrationBuilder.AddColumn<decimal>(
                name: "AccruedInterest",
                table: "Cards",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Cards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "Cards",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MissedPaymentsCount",
                table: "Cards",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccruedInterest",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "MissedPaymentsCount",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "LastReminderSentDate",
                table: "Cards",
                newName: "CreditEndDate");

            migrationBuilder.RenameColumn(
                name: "InterestRate",
                table: "Cards",
                newName: "Percentage");
        }
    }
}
