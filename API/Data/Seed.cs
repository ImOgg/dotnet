using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;
namespace API.Data;

// 資料庫種子資料（Seed Data）
// 在 Program.cs 啟動時呼叫，若資料庫為空則自動填入測試資料
public class Seed
{
    public static async Task SeedData(AppDbContext context)
    {
        // AnyAsync()：檢查 Members 資料表是否已有資料，有的話直接返回避免重複 Seed
        if (await context.Members.AnyAsync()) return;

        // 從 JSON 檔案讀取原始資料（路徑相對於執行目錄，即 API/）
        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        // 反序列化為 DTO 清單；System.Text.Json 預設區分大小寫，JSON key 需與屬性名完全一致
        var members = JsonSerializer.Deserialize<List<SeedUserDTo>>(memberData);

        if (members == null)
        {
            Console.WriteLine("Failed to deserialize member data.");
            return;
        }
        foreach (var member in members)
        {
            // 每個使用者各自 new 一個 HMACSHA512，確保每人的 Salt（Key）都不同
            // using：方法結束時自動呼叫 Dispose() 釋放非託管資源
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email,
                DisplayName = member.DisplayName,
                ImageUrl = member.ImageUrl,
                // 種子資料統一用固定密碼 "Password"，實際登入時用相同流程驗證
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password")),
                PasswordSalt = hmac.Key,
                Member = new Member
                {
                    // Shared PK：Member.Id 設為與 AppUser.Id 相同
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    Description = member.Description,
                    DateofBirth = member.DateofBirth,
                    ImageUrl = member.ImageUrl,
                    Gender = member.Gender,
                    City = member.City,
                    Country = member.Country,
                    LastActive = member.LastActive,
                    Created = member.Created
                }
            };

            // 將 ImageUrl 作為第一張照片加入 Member.Photos
            // member.ImageUrl! — ! 是 null-forgiving operator，告訴編譯器此處確定不為 null
            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                MemberId = member.Id
            });

            // 只需 Add AppUser，EF Core 會透過導航屬性自動追蹤並插入 Member 與 Photo
            context.Users.Add(user);
        }
        await context.SaveChangesAsync();
    }
}
