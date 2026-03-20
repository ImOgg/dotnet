using System.Security.Claims;

// 【為什麼要建立擴充方法（Extension Methods）？】
// 問題：在每個需要取得使用者 Id 的 Controller 方法裡，都要寫這段重複的程式碼：
//   var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//   if (memberId == null) return BadRequest("no id found");
//
// 解法：把這個邏輯封裝成 ClaimsPrincipal 的擴充方法，
// 讓 Controller 可以直接呼叫 User.GetMemberId()，更簡潔、更不容易遺漏錯誤處理。
//
// 【為什麼用 static class？】
// C# 規定擴充方法必須定義在非泛型、非巢狀的靜態類別中。
// static class 無法被繼承或實例化，符合「工具類別」的語意。

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    // this ClaimsPrincipal user：這是擴充方法的語法，
    // 第一個參數前加 this，代表「擴充 ClaimsPrincipal 這個型別」，
    // 讓任何 ClaimsPrincipal 物件都能直接呼叫這個方法（如 User.GetMemberId()）。
    public static string GetMemberId(this ClaimsPrincipal user)
    {
        // 為什麼用 throw 而不是 return null？
        // 因為 JWT Token 正確驗證後，NameIdentifier Claim 一定存在（在 AccountController 登入時就寫入了）。
        // 如果這裡找不到，代表程式邏輯有嚴重問題（Token 配置錯誤），
        // 應該用例外中斷執行，而不是讓呼叫端收到 null 再做額外判斷，
        // 這樣可以避免遺漏 null 檢查導致的 NullReferenceException。
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("User Id claim not found");
    }
}
