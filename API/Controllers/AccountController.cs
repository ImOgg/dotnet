using System.Text;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using API.Extemsions;


namespace API.Controllers;

// AccountController：處理使用者帳號相關的 API（註冊、登入）
//
// 【建構子注入】
// - AppDbContext context：存取資料庫
// - ITokenService tokenService：產生 JWT Token（依賴介面而非具體類別，方便測試與替換）
public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    // POST api/account/register — 使用者註冊
    //
    // 【為什麼用 RegisterDTO 而非 AppUser？】
    // 直接接收 AppUser 會讓前端誤以為需要傳 PasswordHash/Salt 等欄位，
    // 且萬一 API 有 binding 問題，可能讓攻擊者覆蓋不應被設定的欄位（Mass Assignment 攻擊）。
    // RegisterDTO 只暴露「前端應該傳入」的欄位：DisplayName、Email、Password。
    //
    // 【回傳 UserDTO 而非 AppUser？】
    // AppUser 含有 PasswordHash、PasswordSalt 等敏感欄位，絕不能回傳給前端。
    // UserDTO 只包含安全的公開資訊，並附上 JWT Token 讓前端不需再次登入。
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
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 回傳 UserDTO：包含 JWT Token，讓前端註冊後立即取得身份驗證憑證
         return user.ToDto(tokenService);
    }

    // POST api/account/login — 使用者登入
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

        // return new UserDTO
        // {
        //     Id = user.Id,
        //     Email = user.Email,
        //     DisplayName = user.DisplayName,
        //     ImageUrl = user.ImageUrl,
        //     Token = tokenService.CreateToken(user)
        // };
        return user.ToDto(tokenService);
    }

    // 私有輔助方法：檢查 Email 是否已被使用（大小寫不敏感）
    private async Task<bool> UserExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
