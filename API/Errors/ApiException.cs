
namespace API.Errors;

// API 統一錯誤回應模型
// 當 API 發生錯誤時，序列化成 JSON 回傳給前端
// 通常由 ExceptionMiddleware 建立並寫入 Response Body
//
// 使用 C# 12 Primary Constructor 語法：
//   參數直接寫在類名後面，等同於定義建構子
//   每個屬性再用 = 參數名 來初始化，避免重複撰寫傳統建構子方法體
//
// 範例輸出 JSON：
//   { "statusCode": 500, "message": "Internal Server Error", "details": "..." }
public class ApiException(int statusCode, string message, string? details = null)
{
    public int StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = message;

    // string? 表示 nullable，Details 可以為 null（預設值 = null，呼叫時可省略此參數）
    public string? Details { get; set; } = details;
}
