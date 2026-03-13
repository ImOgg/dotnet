using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace API.Entities;

// 照片實體
// 多對一關係：多張 Photo 屬於同一個 Member
public class Photo
{
    // 自增整數主鍵（int → EF Core 自動設為 IDENTITY/AUTO_INCREMENT）
    public int Id { get; set; }

    // 圖片存放位置（本地路徑 或 Cloudinary URL）
    public required string Url { get; set; }

    // Cloudinary 上的資源 ID，用於刪除雲端圖片；本地圖片時為 null
    public string? PublicId { get; set; }

    // 外鍵欄位：指向 Member.Id
    public string MemberId { get; set; } = null!;

    [JsonIgnore] // 序列化時忽略此屬性，避免循環參考問題
    // 導航屬性：反向指向所屬的 Member
    // [ForeignKey("MemberId")] 明確告知 EF Core 用 MemberId 欄位建立關聯
    [ForeignKey("MemberId")]
    public Member Member { get; set; } = null!;
}
