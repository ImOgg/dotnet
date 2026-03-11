// 【TokenService】JWT Token 產生服務
//
// 職責：依據使用者資料，產生一組有效期 7 天的 JWT Token 字串。
//
// 【相依項目】
// - IConfiguration：從 appsettings.json 讀取 "TokenKey"（金鑰，需 >= 64 字元）
// - NuGet：System.IdentityModel.Tokens.Jwt（需手動安裝）
//          Microsoft.AspNetCore.Authentication.JwtBearer（需手動安裝）
//
// 【JWT 結構 — Header.Payload.Signature】
// Header    → 演算法類型（HS512）
// Payload   → Claims（Email、UserId），前端可解碼但無法偽造
// Signature → 用 TokenKey 簽名，後端用來驗證 Token 未被篡改

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        // 從設定檔讀取金鑰；若不存在則拋出例外，避免產生不安全的 Token
        var tokenkey = config["TokenKey"] ?? throw new ArgumentException("Token key is missing in configuration");

        // 金鑰長度至少 64 字元，確保 HMAC-SHA512 的安全強度
        if (tokenkey.Length < 64)
        {
            throw new ArgumentException("Invalid token key length");
        }

        // SymmetricSecurityKey：對稱金鑰，簽名與驗證都使用同一把 Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenkey));

        // Claims（宣告）：嵌入 Token Payload 的使用者資訊
        // 前端可以 Base64 解碼 Payload 取得這些值，但無法偽造（因為沒有 Key 無法重新簽名）
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        // SigningCredentials：指定用哪把 Key 和哪種演算法來簽名
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // SecurityTokenDescriptor：描述 Token 的完整內容
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), // Payload 中的使用者宣告
            Expires = DateTime.Now.AddDays(7),    // Token 有效期：7 天後過期
            SigningCredentials = creds
        };

        // JwtSecurityTokenHandler：負責建立並序列化成最終的 JWT 字串
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token); // 輸出格式：Header.Payload.Signature
    }
}
