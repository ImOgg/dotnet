using System;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using API.DTOs;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers;

public class AccountController(AppDbContext context) : BaseApiController
{
    [HttpPost("register")] // api/account/register

    // 使用 RegisterDTO 作為參數，從 HTTP 請求的 body 中接收註冊資料
    // 這樣做的好處是可以清楚定義前端需要提供哪些資料，並且在後端進行驗證和轉換
    // 原本在前端直接傳送 AppUser 物件，這樣不太安全也不靈活，因為 AppUser 包含了密碼雜湊和鹽值等敏感資訊

    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
        if (await UserExists(registerDTO.Email)) return BadRequest("Email already in use");

        var hmac = new HMACSHA512();
        var user = new AppUser
        {
            DisplayName = registerDTO.DisplayName,
            Email = registerDTO.Email,
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
       
        return Ok(user);
    }

    private async Task<bool> UserExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}