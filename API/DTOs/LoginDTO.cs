using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;
public class LoginDTO
{

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
