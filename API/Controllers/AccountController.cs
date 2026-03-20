using System.Text;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using API.Extensions;


namespace API.Controllers;

// AccountController：處理使用者帳號相關的 API（註冊、登入）
//
// 【建構子注入】
// - AppDbContext context：存取資料庫
// - ITokenService tokenService：產生 JWT Token（依賴介面而非具體類別，方便測試與替換）
//
// 【為什麼不直接用 AppUser？】
// 前端傳入 AppUser  → 有 Mass Assignment 風險（覆蓋 PasswordHash 等欄位）
// 後端回傳 AppUser  → 洩漏 PasswordHash、PasswordSalt 等敏感欄位
// 正確做法：輸入用 RegisterDTO，輸出用 UserDTO。
//
// 【user.ToDto(tokenService) 是什麼？】
// AppUserExtensions.cs 中定義的擴充方法，
// 將 AppUser 轉換為 UserDTO，Register / Login 共用同一段轉換邏輯，避免重複程式碼。
public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    // POST api/account/register — 使用者註冊
    //
    // 流程：
    //   前端 POST /api/account/register { DisplayName, Email, Password }
    //          ↓
    //   1. 檢查 Email 是否已存在（UserExists）
    //   2. new HMACSHA512() → 自動產生隨機 Key 作為鹽
    //   3. PasswordHash = hmac.ComputeHash(UTF8(Password))
    //   4. PasswordSalt = hmac.Key
    //   5. 存入資料庫
    //   6. user.ToDto(tokenService) → 回傳 UserDTO（含 JWT Token）

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {
        // 若 Email 已存在則拒絕註冊
        if (await UserExists(registerDTO.Email)) return BadRequest("Email already in use");

        // HMACSHA512：使用隨機產生的 Key 作為「鹽」，對密碼做雜湊
        // using 確保 hmac 用完後釋放資源（實作 IDisposable）
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            DisplayName = registerDTO.DisplayName,
            Email = registerDTO.Email,
            // 將密碼轉成 UTF-8 bytes 後雜湊；每次 new HMACSHA512() 的 Key 都不同，
            // 所以相同密碼雜湊出的結果也不同（防彩虹表攻擊）
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            // 儲存 Key（鹽），登入時需要用同一把 Key 重現雜湊來驗證密碼
            PasswordSalt = hmac.Key,
            Member = new Member
            {
                DisplayName = registerDTO.DisplayName,
                Gender = registerDTO.Gender,
                City = registerDTO.City,
                Country = registerDTO.Country,
            }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 回傳 UserDTO：包含 JWT Token，讓前端註冊後立即取得身份驗證憑證
        return user.ToDto(tokenService);
    }
    // POST api/account/login — 使用者登入
    //
    // 流程：
    //   前端 POST /api/account/login { Email, Password }
    //          ↓
    //   1. 用 Email 查詢使用者（SingleOrDefaultAsync）
    //   2. new HMACSHA512(user.PasswordSalt) → 用儲存的鹽重建 HMAC
    //   3. computedHash = hmac.ComputeHash(UTF8(輸入密碼))
    //   4. 逐 byte 比對 computedHash vs user.PasswordHash
    //   5. 比對通過 → user.ToDto(tokenService) → 回傳 UserDTO（含 JWT Token）
    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        // SingleOrDefaultAsync：查詢單一筆符合 Email 的使用者
        // - 找到一筆 → 回傳該物件
        // - 找不到  → 回傳 null（不會拋例外）
        // - 找到多筆 → 拋 InvalidOperationException（與 FirstOrDefaultAsync 的差異）
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDTO.Email);
        if (user == null) return Unauthorized("Invalid email");

        // 用資料庫儲存的 PasswordSalt 重建 HMAC 實例，
        // 才能對使用者輸入的密碼做出「相同的雜湊結果」來比對
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        // 逐 byte 比對雜湊結果（不能用 == 比較陣列，那只比較參考位址）
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        // 回傳 UserDTO：包含 JWT Token，讓前端登入後立即取得身份驗證憑證
        return user.ToDto(tokenService);
    }

    // 私有輔助方法：檢查 Email 是否已被使用（大小寫不敏感）
    private async Task<bool> UserExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
