using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extemsions;

// 【擴充方法類別】
// 為 AppUser Entity 新增轉換方法，不需修改原始類別。
// 使用方式：user.ToDto(tokenService)
public static class AppUserExtensions
{
    // 將 AppUser Entity 轉換為 UserDTO
    //
    // 【為什麼用擴充方法？】
    // Register 和 Login 都需要回傳相同結構的 UserDTO，
    // 把轉換邏輯抽到這裡，Controller 只需一行 user.ToDto(tokenService)，
    // 避免重複程式碼，日後只需在這裡維護。
    //
    // 【this AppUser user】
    // this 關鍵字表示這個方法「掛在」AppUser 型別上，
    // 讓 AppUser 的實例可以直接呼叫 .ToDto()。
    public static UserDTO ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
            // 呼叫 ITokenService 產生 JWT Token，讓前端取得後直接使用
            Token = tokenService.CreateToken(user)
        };
    }
}
