using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Members_SourceUserId",
                table: "Likes");

            migrationBuilder.RenameColumn(
                name: "SourceUserId",
                table: "Likes",
                newName: "SourceMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Members_SourceMemberId",
                table: "Likes",
                column: "SourceMemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Members_SourceMemberId",
                table: "Likes");

            migrationBuilder.RenameColumn(
                name: "SourceMemberId",
                table: "Likes",
                newName: "SourceUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Members_SourceUserId",
                table: "Likes",
                column: "SourceUserId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
