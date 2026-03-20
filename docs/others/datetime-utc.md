# DateTime UTC 轉換 與 LastActive 功能

## 為什麼需要 UTC 轉換器？

MySQL 儲存 `DateTime` 時**不帶時區資訊**，EF Core 讀取後 `Kind` 預設為 `Unspecified`。

```csharp
// ❌ 沒有轉換器時，Kind = Unspecified，比較會錯誤或拋出例外
var diff = DateTime.UtcNow - member.LastActive;
//         Kind = Utc        Kind = Unspecified → 結果不可靠

// ✅ 有轉換器後，Kind = Utc，計算正確
var diff = DateTime.UtcNow - member.LastActive; // 正常運作
```

## AppDbContext 的轉換器設定

```csharp
// OnModelCreating 內
var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
    v => v,                                          // 寫入 DB：不動
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)  // 讀取 DB：標記為 UTC
);

// 自動套用至所有 Entity 的所有 DateTime 屬性
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    foreach (var property in entityType.GetProperties())
        if (property.ClrType == typeof(DateTime))
            property.SetValueConverter(dateTimeConverter);
```

> 不需要每個 Entity 各自設定，一次套用全部。

---

## 延伸功能：「多久前活躍」

### 1. Entity 加上 LastActive 欄位

```csharp
// Entities/Member.cs
public DateTime LastActive { get; set; } = DateTime.UtcNow;
```

### 2. DTO 回傳給前端

```csharp
// DTOs/MemberDto.cs
public DateTime LastActive { get; set; }
```

### 3. 登入時更新 LastActive

可透過 Action Filter 或 Middleware 自動更新，不必每個 endpoint 手動寫：

```csharp
// Helpers/LogUserActivity.cs
public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        var userId = resultContext.HttpContext.User.GetUserId();
        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetMemberByIdAsync(userId);
        user.LastActive = DateTime.UtcNow;
        await repo.SaveAllAsync();
    }
}
```

在 `Program.cs` 註冊：

```csharp
builder.Services.AddScoped<LogUserActivity>();
```

在 Controller 或全域套用：

```csharp
[ServiceFilter(typeof(LogUserActivity))]
public class MembersController : BaseApiController { ... }
```

### 4. 前端顯示相對時間

後端序列化出去的 UTC 時間帶 `Z`（ISO 8601）：

```
"lastActive": "2026-03-20T08:00:00Z"
```

前端用 `date-fns` 或 `dayjs` 顯示：

```js
// date-fns
import { formatDistanceToNow } from 'date-fns'
formatDistanceToNow(new Date(member.lastActive), { addSuffix: true })
// → "3 hours ago" / "2 days ago"

// dayjs
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)
dayjs(member.lastActive).fromNow() // → "3 小時前"
```

---

## 完整資料流

```
使用者發送請求
    ↓
LogUserActivity Filter → 更新 LastActive = DateTime.UtcNow
    ↓
MySQL 存入（無時區資訊）
    ↓
EF Core 讀取 → ValueConverter 標記 Kind = Utc ✅
    ↓
序列化成 "2026-03-20T08:00:00Z"（帶 Z）
    ↓
前端 new Date("...Z") → 自動轉換成本地時區
    ↓
顯示「3 小時前」
```

## 重點整理

| 情境 | 沒有 UTC 轉換器 | 有 UTC 轉換器 |
|------|----------------|--------------|
| `DateTime.UtcNow - LastActive` | 結果不可靠 | 正確 |
| JSON 序列化 | 不帶 `Z`，前端時區錯誤 | 帶 `Z`，前端正確解析 |
| 跨時區部署 | 時間顯示混亂 | 統一 UTC，無問題 |
