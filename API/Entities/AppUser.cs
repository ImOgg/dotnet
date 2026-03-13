namespace API.Entities;

// 認證使用者實體（Authentication 層）
// 儲存登入相關資料：帳號、密碼雜湊、Token 所需欄位
// 與 Member 是 1-to-1 關係，Member 負責儲存個人資料（Profile 層）
public class AppUser
{
    // 主鍵：建立物件時自動產生唯一的 UUID 字串（e.g. "550e8400-e29b-41d4-a716-446655440000"）
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string DisplayName { get; set; }

    public required string Email { get; set; }
    public string? ImageUrl { get; set; }

    // 密碼不儲存明文，改用 HMACSHA512 演算法雜湊後存入
    // PasswordHash = HMAC(明文密碼) — 驗證登入時重新計算比對
    // PasswordSalt = HMAC 實例的 Key  — 每個使用者不同，防止彩虹表攻擊
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }

    // 導航屬性：對應 Member 實體（一對一）
    // EF Core 會透過 Member.Id（FK）自動建立關聯，不需要額外宣告 FK 欄位
    public Member Member { get; set; } = null!;

}
