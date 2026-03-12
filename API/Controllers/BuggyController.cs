using System;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// 【BuggyController 說明】
// 這個 Controller 的目的是「刻意觸發各種 HTTP 錯誤」，用來測試：
// 1. 前端能否正確解析不同的 HTTP 錯誤狀態碼（401、404、500、400）
// 2. 全局異常處理中介軟體（ExceptionMiddleware）能否攔截未處理的例外並回傳統一格式
// 在學習/開發階段，有一個可以主動引發錯誤的端點，比等待偶發 bug 方便得多。
public class BuggyController : BaseApiController
{
    // 模擬「未授權」的情境（HTTP 401）
    // 用途：驗證前端的 HTTP 攔截器是否能正確捕捉 401，並導向登入頁面或顯示錯誤訊息
    // 回傳 ProblemDetails 是 ASP.NET Core 標準的錯誤格式（RFC 7807）
    [HttpGet("auth")]
    public IActionResult GetSecret()
    {
        return Unauthorized(new ProblemDetails { Title = "You are not authorized" });
    }

    // 模擬「資源找不到」的情境（HTTP 404）
    // 用途：驗證前端對 404 的處理邏輯（例如顯示「找不到頁面」的提示）
    [HttpGet("not-found")]
    public ActionResult GetNotFound()
    {
        return NotFound();
    }

    // 模擬「伺服器內部錯誤」的情境（HTTP 500）
    // 用途：驗證全局 ExceptionMiddleware 能否攔截未處理的 Exception，
    //       並回傳格式統一的錯誤 JSON，而不是讓 ASP.NET Core 預設的錯誤頁面洩漏 stack trace
    // 注意：這裡是「主動 throw」，模擬程式碼中意外拋出的例外
    [HttpGet("server-error")]
    public ActionResult GetServerError()
    {
        throw new Exception("This is a server error");
    }

    // 模擬「請求格式錯誤」的情境（HTTP 400）
    // 用途：驗證前端對 400 的處理邏輯（例如顯示使用者輸入有誤的提示）
    [HttpGet("bad-request")]
    public ActionResult GetBadRequest()
    {
        return BadRequest(new ProblemDetails { Title = "This is a bad request" });
    }
}