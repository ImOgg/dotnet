namespace API.DTOs;

// Seed 專用 DTO：對應 UserSeedData.json 的欄位結構
// 用途：將 JSON 反序列化成 C# 物件，再轉為 AppUser + Member 存入資料庫
//
// 命名規則注意：
//   System.Text.Json 預設區分大小寫，DTO 屬性名稱必須與 JSON key 完全一致
//   例如 JSON 是 "Id" → C# 必須是 Id（不能是 id）
public class SeedUserDTo
{
    public required string Id { get; set; }

    public required string Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public DateTime Created { get; set; }

    public DateTime LastActive { get; set; }

    public required string Gender { get; set; }

    public string Description { get; set; } = null!;

    public required string City { get; set; }

    public required string Country { get; set; }
}
