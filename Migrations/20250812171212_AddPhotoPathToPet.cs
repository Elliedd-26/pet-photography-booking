using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPhotographyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoPathToPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Pets",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Pets");
        }
    }
}
