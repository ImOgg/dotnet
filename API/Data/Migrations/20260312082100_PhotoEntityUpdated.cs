using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhotoEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Users_id",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Members_Memberid",
                table: "Photos");

            migrationBuilder.RenameColumn(
                name: "Memberid",
                table: "Photos",
                newName: "MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Photos_Memberid",
                table: "Photos",
                newName: "IX_Photos_MemberId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Members",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Users_Id",
                table: "Members",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Members_MemberId",
                table: "Photos",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Users_Id",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Members_MemberId",
                table: "Photos");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Photos",
                newName: "Memberid");

            migrationBuilder.RenameIndex(
                name: "IX_Photos_MemberId",
                table: "Photos",
                newName: "IX_Photos_Memberid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Members",
                newName: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Users_id",
                table: "Members",
                column: "id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Members_Memberid",
                table: "Photos",
                column: "Memberid",
                principalTable: "Members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
