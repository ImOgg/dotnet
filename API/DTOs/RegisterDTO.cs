using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

// DTO（Data Transfer Object）：用來接收前端傳來的註冊請求資料
// 只包含必要欄位，避免直接暴露 Entity（AppUser）
public class RegisterDTO
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    // 必填：使用者 Email（作為帳號）
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // 必填：密碼
    [Required]
    // 必填：最少 6 個字元
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

}
