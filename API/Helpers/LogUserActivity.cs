using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Extemsions;

namespace API.Helpers;

// 【LogUserActivity 是什麼？】
// 這是一個 Action Filter，實作 IAsyncActionFilter 介面。
// 每次有 API 請求被處理時，這個 Filter 會自動執行，
// 用來記錄「已登入使用者最後一次活動的時間（LastActive）」。
//
// 【為什麼用 Filter 而不是在每個 Controller 裡寫？】
// 如果每個 Controller Action 都要更新 LastActive，程式碼會非常重複。
// 用 Filter 可以把這個邏輯集中管理，只要在 Program.cs 全域註冊一次，
// 所有 Controller 的 Action 都會自動套用，符合 DRY 原則。
//
// 【如何啟用？】
// 在 Program.cs 加入：
//   builder.Services.AddControllers(opt =>
//   {
//       opt.Filters.Add<LogUserActivity>();
//   });
public class LogUserActivity : IAsyncActionFilter
{
    // OnActionExecutionAsync 是 IAsyncActionFilter 規定要實作的方法。
    // context：包含這次請求的資訊（HttpContext、Route、User 等）
    // next：代表「繼續執行下一個 Filter 或 Controller Action」的委派
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 先呼叫 await next()，讓 Controller Action 正常執行完畢，
        // 再於「Action 執行之後」進行 LastActive 的更新（Post-Action 邏輯）。
        // resultContext 包含 Action 執行完的結果與 HttpContext。
        var resultContext = await next();

        // 如果使用者尚未登入（沒有通過 JWT 驗證），就直接結束，不做任何更新。
        // 只有已驗證的使用者才需要記錄活動時間。
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;

        // 從 JWT Token 的 Claim 中取出當前使用者的 Id。
        // GetMemberId() 是定義在 ClaimsPrincipalExtensions 的擴充方法。
        var memberId = resultContext.HttpContext.User.GetMemberId();

        // 透過 DI 容器從 RequestServices 取得 DbContext。
        // 這裡不用建構子注入，是因為 Filter 預設是 Singleton 生命週期，
        // 而 DbContext 是 Scoped，直接注入會造成生命週期衝突（Captive Dependency）。
        // 改從 RequestServices 取得可確保每次請求都拿到正確的 Scoped 實例。
        var dbContext = resultContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        // 用 ExecuteUpdateAsync 直接在資料庫層執行 UPDATE，
        // 不需要先把 Member 載入記憶體，效能比 SaveChanges 更好。
        // 只更新 LastActive 欄位為目前的 UTC 時間。
        await dbContext.Members.Where(m => m.Id == memberId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.LastActive, DateTime.UtcNow));
    }
}
