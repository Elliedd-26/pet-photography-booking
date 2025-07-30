using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPhotographyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipientOwnerIdToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Owners_OwnerId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Notifications",
                newName: "RecipientOwnerId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_OwnerId",
                table: "Notifications",
                newName: "IX_Notifications_RecipientOwnerId");

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Owners_RecipientOwnerId",
                table: "Notifications",
                column: "RecipientOwnerId",
                principalTable: "Owners",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Owners_RecipientOwnerId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Notifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "RecipientOwnerId",
                table: "Notifications",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RecipientOwnerId",
                table: "Notifications",
                newName: "IX_Notifications_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Owners_OwnerId",
                table: "Notifications",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "OwnerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
