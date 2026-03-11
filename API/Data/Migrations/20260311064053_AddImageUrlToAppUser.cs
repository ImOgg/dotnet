using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    // 【Migration 說明】AddImageUrlToAppUser
    // 這次 migration 做了兩件事：
    //   1. 將 PasswordHash / PasswordSalt 從 nullable 改成 NOT NULL
    //      → AppUser.cs 已把這兩個欄位標記為 required，需與資料庫保持一致
    //   2. 新增 ImageUrl 欄位（TEXT，nullable）
    //      → 使用者可選擇性上傳大頭照，未上傳時為 null
    public partial class AddImageUrlToAppUser : Migration
    {
        // Up()：執行 dotnet ef database update 時套用的變更（升級）
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PasswordSalt：nullable → NOT NULL（配合 AppUser.cs required 關鍵字）
            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "Users",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            // PasswordHash：nullable → NOT NULL（同上）
            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "Users",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            // 新增 ImageUrl 欄位（TEXT，允許 null）
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        // Down()：執行 dotnet ef database update <上一個Migration名稱> 時回滾用（降級）
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 移除 ImageUrl 欄位
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Users");

            // PasswordSalt / PasswordHash 還原為 nullable
            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "Users",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "Users",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");
        }
    }
}
