using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QUinoCloud.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandInfos_AspNetUsers_OwnerId",
                table: "CommandInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaCatalogs_AspNetUsers_OwnerId",
                table: "MediaCatalogs");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "MediaInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "MediaCatalogs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "CommandInfos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandInfos_AspNetUsers_OwnerId",
                table: "CommandInfos",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaCatalogs_AspNetUsers_OwnerId",
                table: "MediaCatalogs",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandInfos_AspNetUsers_OwnerId",
                table: "CommandInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaCatalogs_AspNetUsers_OwnerId",
                table: "MediaCatalogs");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "MediaInfos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "MediaCatalogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "CommandInfos",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CommandInfos_AspNetUsers_OwnerId",
                table: "CommandInfos",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaCatalogs_AspNetUsers_OwnerId",
                table: "MediaCatalogs",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
