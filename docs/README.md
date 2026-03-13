## 資料夾介紹與職責

| 資料夾 | 職責 |
|---|---|
| **Controllers** | HTTP 端點（Route + Action），接收請求、回傳回應，不寫業務邏輯 |
| **Entities** | 對應資料庫表格的 Model（AppUser、Member、Photo） |
| **Data** | DbContext（資料庫連線）、Repository（資料存取實作）、Migrations、Seed |
| **Interfaces** | 抽象介面定義（IMemberRepository、ITokenService） |
| **DTOs** | 控制「進出 API 的資料形狀」，避免直接暴露 Entity |
| **Services** | 共用業務邏輯（TokenService 產生 JWT） |
| **Errors** | 自訂錯誤格式（ApiException，統一 JSON 結構） |
| **Extemsions** | 擴充方法（Extension Methods），例如 AppUser.ToDto() |
| **middleware** | 中介軟體（ExceptionMiddleware，全局捕捉例外） |
| **Properties** | launchSettings.json，設定本地開發的埠號與環境變數 |

---

## 各層之間怎麼串起來？

### 一次完整的 HTTP 請求流程

```
前端請求 GET /api/members
        ↓
[Program.cs] — 路由分配，把請求交給對應的 Controller
        ↓
[MembersController] — 收到請求，呼叫 IMemberRepository.GetMembersAsync()
        ↓
[IMemberRepository] — 介面（合約），Controller 只認識這層，不知道底下是誰
        ↓  ← DI 容器在這裡決定：實際給你的是 MemberRepository
[MemberRepository] — 實際執行，透過 AppDbContext 查資料庫
        ↓
[AppDbContext] — EF Core 把 DbSet<Member> 翻譯成 SQL，查 MySQL/SQLite
        ↓
[Member Entity] — 查詢結果對應到 C# 物件後回傳
        ↓
Controller 回傳 Ok(members) → 序列化成 JSON → 前端收到回應
```

---

## 幾個核心概念解釋

### DbSet 是什麼？

`DbSet<Member>` 就是「資料庫表格的代理物件」。

```csharp
// AppDbContext.cs
public DbSet<AppUser> Users { get; set; }   // 代表 Users 表
public DbSet<Member> Members { get; set; }  // 代表 Members 表
public DbSet<Photo> Photos { get; set; }    // 代表 Photos 表
```

你對 `DbSet` 下的 LINQ 查詢，EF Core 會自動翻譯成 SQL 語句執行：

```csharp
context.Members.ToListAsync()         // → SELECT * FROM Members
context.Members.FindAsync(id)         // → SELECT * FROM Members WHERE Id = @id
context.Members.Where(m => m.Id == x) // → SELECT * FROM Members WHERE Id = @x
```

你不用自己寫 SQL，EF Core 幫你搞定。

---

### IMemberRepository 和 MemberRepository 的關係

這是「依賴介面，而非依賴實作」的設計模式，稱為 **Repository Pattern**。

```
IMemberRepository（介面）   ←  定義「能做什麼」（合約）
        ↑ 實作
MemberRepository（實作）    ←  定義「怎麼做」（具體邏輯）
```

**為什麼要多一層介面？**

1. **Controller 不知道、也不需要知道資料是怎麼查的**：
   ```csharp
   // MembersController 只依賴介面
   public class MembersController(IMemberRepository memberRepository) : BaseApiController
   {
       var members = await memberRepository.GetMembersAsync(); // 不管底層怎麼實作
   }
   ```

2. **Program.cs 決定「誰來扮演這個介面」**：
   ```csharp
   // 告訴 DI 容器：當有人要 IMemberRepository，就給他 MemberRepository
   builder.Services.AddScoped<IMemberRepository, MemberRepository>();
   ```

3. **好處**：測試時可以替換成假的實作（Mock），不需要真實資料庫。未來換掉 MemberRepository 的內部邏輯，Controller 完全不需要改。

類比：IMemberRepository 像「插座規格（110V）」，MemberRepository 像「符合規格的電器」。Controller 只管插上去用，不管裡面怎麼導電。

---

### DI（依賴注入）是怎麼運作的？

```csharp
// Program.cs — 服務註冊（告訴 DI 容器有哪些服務）
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddDbContext<AppDbContext>(...);
```

當 Controller 的建構子宣告需要某個介面，DI 容器會**自動建立對應的實作並注入進去**：

```csharp
// Controller 的建構子只寫「需要什麼」，DI 自動提供
public class MembersController(IMemberRepository memberRepository) : BaseApiController
public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
```

你不需要自己 `new MemberRepository()`，DI 框架幫你做。

---

### DTOs 為什麼不直接用 Entity？

```csharp
// 壞寫法：直接回傳 AppUser（把 PasswordHash、PasswordSalt 也送出去了！）
return Ok(user);

// 好寫法：回傳 UserDTO（只包含前端需要的安全欄位）
return user.ToDto(tokenService); // Extensions 裡的擴充方法
```

| | Entity (AppUser) | DTO (UserDTO) |
|---|---|---|
| 用途 | 對應資料庫 | 控制 API 輸出格式 |
| 包含敏感欄位 | ✅ PasswordHash、PasswordSalt | ❌ 只有安全欄位 |
| 前端看到 | 不應該直接看到 | 就是給前端看的 |

RegisterDTO、LoginDTO 同理：限制前端「能傳入哪些欄位」，防止 Mass Assignment 攻擊。

---

### Middleware（中介軟體）的執行順序

`Program.cs` 裡的 `app.UseXxx()` 就是在組裝「請求管線」，順序非常重要：

```
HTTP 請求進來
    ↓
ExceptionMiddleware   ← 包裹整個管線，捕捉任何層拋出的例外
    ↓
UseCors              ← 允許前端跨域請求
    ↓
UseAuthentication     ← 驗證 JWT Token（我是誰？）
    ↓
UseAuthorization      ← 檢查權限（我能做這件事嗎？）
    ↓
MapControllers        ← 路由分配到對應的 Controller Action
    ↓
HTTP 回應出去
```

**為什麼 Authentication 要在 Authorization 前面？**
因為要先知道「你是誰」，才能判斷「你能不能做這件事」。順序反了就無法正確授權。

---

### WeatherForecast.cs 是什麼？

這是 `dotnet new webapi` 建立專案時自動產生的**範例程式碼**，展示一個最簡單的 API 端點長什麼樣子。實際開發不會用到，可以保留當參考，或直接刪除。

---

## 整體架構圖

```
appsettings.json
      ↓ (設定)
Program.cs ─────── 服務註冊（DI）+ Middleware 管線
      ↓
[請求進來]
      ↓
Middleware（ExceptionMiddleware → Authentication → Authorization）
      ↓
Controller（MembersController / AccountController）
   ↙        ↘
Interface    DTO（控制輸入/輸出格式）
   ↓
Repository（MemberRepository）
   ↓
AppDbContext（EF Core）
   ↓
DbSet<Entity>（AppUser / Member / Photo）
   ↓
資料庫（SQLite / MySQL）
```

**Services**（TokenService）是跨層共用的，任何層都可以透過 DI 注入使用。
**Errors + Middleware** 合作，讓例外被全局攔截並回傳統一格式的 JSON。
**Migrations** 是 EF Core 追蹤 Entity 結構變化的記錄，`dotnet ef database update` 才會真正建立/更新資料庫表格。
