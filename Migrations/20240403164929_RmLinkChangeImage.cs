using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventLink.Migrations
{
    /// <inheritdoc />
    public partial class RmLinkChangeImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgUrl",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "Events",
                newName: "Image");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Events",
                newName: "Link");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImgUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
