using System.Text;
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

    public async Task<ActionResult<AppUser>> Register(RegisterDTO registerDTO)
    {
        if (await UserExists(registerDTO.Email)) return BadRequest("Email already in use");

        var hmac = new HMACSHA512();
        var user = new AppUser
        {
            DisplayName = registerDTO.DisplayName,
            Email = registerDTO.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
       
        return Ok(user);
    }
    [HttpPost("login")] // api/account/login
    public async Task<ActionResult<AppUser>> Login(LoginDTO loginDTO)
    {
        // SingleOrDefaultAsync：從資料庫查詢符合條件的「單一筆」資料
        // - 若找到一筆 → 回傳該物件
        // - 若找不到  → 回傳 null（預設值），不會拋例外
        // - 若找到多筆 → 拋出 InvalidOperationException（與 FirstOrDefaultAsync 的差異）
        // 來自 Microsoft.EntityFrameworkCore 命名空間（EF Core 的擴充方法）
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDTO.Email);
        if (user == null) return Unauthorized("Invalid email");

        var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        return Ok(user);
    }

    private async Task<bool> UserExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}