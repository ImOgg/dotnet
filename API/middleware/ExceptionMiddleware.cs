
using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

// 【ExceptionMiddleware 說明】
// 這是一個「全局異常處理中介軟體」，負責攔截整個請求管線中所有未處理的 Exception。
//
// 為什麼需要這個？
// ASP.NET Core 預設在未捕獲例外時，會回傳 HTML 格式的錯誤頁面（開發環境）或空白 500（生產環境）。
// 對於 API 來說，前端期望的是「統一格式的 JSON 錯誤」，而非 HTML，因此需要自訂中介軟體來處理。
//
// 使用 Primary Constructor 注入三個依賴：
// - next          : 管線中的下一個中介軟體（必須呼叫，否則請求不會繼續往下傳遞）
// - logger        : 用來記錄例外的詳細資訊到日誌系統
// - env           : 用來判斷目前是開發還是生產環境，決定回傳多少錯誤細節
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    // 【效能優化】靜態唯讀欄位，整個應用程式生命週期只建立一次。
    // JsonSerializerOptions 是重量級物件，若每次請求都 new 一個，會造成不必要的 GC 壓力。
    // CamelCase：讓序列化結果符合前端 JavaScript 慣例（例如 StatusCode → statusCode）
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ASP.NET Core 中介軟體的核心方法，每個 HTTP 請求都會經過這裡
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // 正常情況：將請求傳遞給管線中的下一個中介軟體（Controller、路由等）
            await next(context);
        }
        catch (Exception ex)
        {
            // 將例外寫入日誌，方便排查問題（使用結構化日誌，{message} 是具名的佔位符）
            logger.LogError(ex, "{message}", ex.Message);

            // 強制將回應格式設為 JSON，確保前端不會收到意外的 HTML
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // 根據環境決定回傳的錯誤詳細程度：
            // - 開發環境：包含完整 StackTrace，方便開發者除錯
            // - 生產環境：只回傳通用訊息，避免洩漏內部實作細節（安全性考量）
            var response = env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

            var json = JsonSerializer.Serialize(response, JsonOptions);

            await context.Response.WriteAsync(json);
        }
    }
}