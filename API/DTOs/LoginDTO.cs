using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

// DTO（Data Transfer Object）：用來接收前端傳來的登入請求資料
//
// 【與 RegisterDTO 的差異】
// LoginDTO 只需要 Email + Password，不需要 DisplayName，
// 因為登入只驗證身份，不建立新資料。
public class LoginDTO
{
    // 必填：使用者 Email（作為帳號）
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // 必填：密碼、最少 6 個字元
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

}
