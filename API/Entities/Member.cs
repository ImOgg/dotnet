using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Entities;

// 會員個人資料實體（Profile 層）
// 儲存與登入無關的個人資訊（生日、城市、照片等）
// 與 AppUser 是 1-to-1 關係，共用同一個 Id（Shared Primary Key 模式）
//   → Member.Id = AppUser.Id，不額外自增，節省 JOIN 查詢
public class Member
{
    // 主鍵，同時也是指向 AppUser 的外鍵（Shared PK 設計）
    // null! 告訴編譯器：此欄位不會是 null，EF Core 載入時一定會賦值，不需要警告
    public string Id { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string? ImageUrl { get; set; }

    // 預設值 DateTime.Now：建立 / 最後活躍時間在 new 時自動填入
    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime LastActive { get; set; } = DateTime.Now;

    // required：建立物件時必須明確賦值，編譯器會強制檢查
    public required string Gender { get; set; }

    public string Description { get; set; } = null!;

    public required string City { get; set; }

    public required string Country { get; set; }

    [JsonIgnore] // 序列化時忽略此屬性，避免循環參考問題
    // 一對多導航屬性：一個 Member 可有多張照片
    // [] 等同 new List<Photo>()，C# 12 集合表達式語法
    public List<Photo> Photos { get; set; } = [];

    [JsonIgnore] // 序列化時忽略此屬性，避免循環參考問題
    // 一對一導航屬性：反向指向 AppUser
    // [ForeignKey(nameof(Id))] 告訴 EF Core：用 Id 欄位作為此關聯的外鍵
    [ForeignKey(nameof(Id))]
    public AppUser User { get; set; } = null!;
    public string? DisplayName { get; set; }

    // ===== 喜歡與被喜歡的會員列表 =====
    // 一對多導航屬性：一個 Member 可喜歡多個 Member
    [JsonIgnore] // 序列化時忽略此屬性，避免循環參考問題
    public List<MemberLike> LikedByMembers { get; set; } = [];
    [JsonIgnore]
    public List<MemberLike> LikedMembers { get; set; } = [];

    // 訊息
    [JsonIgnore]
    public List<Message> MessagesSent { get; set; } = [];
    [JsonIgnore]
    public List<Message> MessagesReceived { get; set; } = [];
}
