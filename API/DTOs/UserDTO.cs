namespace API.DTOs;

// DTO（Data Transfer Object）：用來回傳給前端的使用者資料
// 同時包含 JWT Token，讓前端登入 / 註冊後可以立即取得身份驗證憑證
//
// 【為什麼不直接回傳 AppUser Entity？】
// AppUser 包含 PasswordHash 和 PasswordSalt，若直接回傳會洩漏敏感資訊。
// UserDTO 只包含前端「真正需要」的欄位，是安全的公開介面。
//
// 【使用情境】
// - POST api/account/register → 回傳此 DTO（含 Token，讓使用者不需再次登入）
// - POST api/account/login    → 回傳此 DTO（含 Token）
public class UserDTO
{
    // 使用者的唯一識別碼（對應 AppUser.Id，UUID 字串格式）
    public string Id { get; set; } = string.Empty;

    // 登入帳號
    public string Email { get; set; } = string.Empty;

    // 顯示名稱（前端展示用）
    public string DisplayName { get; set; } = string.Empty;

    // 大頭照 URL，可為 null（使用者尚未上傳圖片）
    public string? ImageUrl { get; set; } = string.Empty;

    // JWT Token（必填）：前端後續請求需放在 Authorization Header
    // 格式：Bearer <token>
    public required string Token { get; set; } = string.Empty;
}
