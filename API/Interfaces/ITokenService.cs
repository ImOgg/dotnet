// 【介面定義】ITokenService
// 定義 JWT Token 的產生合約，任何實作此介面的類別都必須提供 CreateToken 方法。
// 使用介面的好處：Controller 只依賴介面，而不直接依賴具體實作（TokenService），
// 方便日後替換實作或撰寫單元測試（Mock）。
//
// 【相依項目】
// - API.Entities：需要 AppUser 實體作為參數型別

using API.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    // 傳入使用者資料，回傳一組 JWT Token 字串
    string CreateToken(AppUser user);

}
 