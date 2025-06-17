using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkinRush.Migrations
{
    /// <inheritdoc />
    public partial class EditDotaSkinAndCSGOSkin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImmortal",
                table: "DotaSkins");

            migrationBuilder.DropColumn(
                name: "HasStatTrak",
                table: "CSGOSkins");

            migrationBuilder.RenameColumn(
                name: "Slot",
                table: "DotaSkins",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Weapon",
                table: "CSGOSkins",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Rarity",
                table: "CSGOSkins",
                newName: "Game");

            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "DotaSkins",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Game",
                table: "DotaSkins");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "DotaSkins",
                newName: "Slot");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "CSGOSkins",
                newName: "Weapon");

            migrationBuilder.RenameColumn(
                name: "Game",
                table: "CSGOSkins",
                newName: "Rarity");

            migrationBuilder.AddColumn<bool>(
                name: "IsImmortal",
                table: "DotaSkins",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStatTrak",
                table: "CSGOSkins",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
