### Migration 運作原理

EF Core 比對「Entity 類別」和「上一個 migration 的快照」之間的差異，自動產生 `Up()` / `Down()`。

**順序永遠是：先改 Entity → 再跑 migration**，不能反過來。

```
你改 Entity → dotnet ef migrations add → EF Core 比對差異 → 自動產生 Up()/Down()
```

每次 `dotnet ef migrations add` 會產生兩個檔案：

| 檔案 | 用途 | 需要看嗎 |
|------|------|---------|
| `<名稱>.cs` | 實際的 SQL 操作邏輯（`Up` 升級、`Down` 回滾） | ✅ 要看 |
| `<名稱>.Designer.cs` | EF Core 自用的 model 快照，用來比對下一次差異 | ❌ 不用管，自動產生 |

### Migration 指令

```bash
dotnet ef                                              # 查看所有可用指令
dotnet ef migrations -h                                # 查看 migrations 子指令說明
dotnet ef database drop
dotnet ef database update 0 
```
### 1. 創建 Migration

```bash
dotnet ef migrations add <MigrationName>
```

**範例：**
```bash
dotnet ef migrations add InitialCreate -o Data/Migrations  # 新增 Migration
dotnet ef migrations add UserEntityUpdated #新增 更新migration的檔案
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddUserEmail
dotnet ef migrations add UpdateProductPrice
```

### 2. 套用 Migration 到資料庫

```bash
dotnet ef database update
```
### 3. 查看所有 Migrations

```bash
dotnet ef migrations list
```

**輸出範例：**
```
20251020071659_InitialCreate
20251020090313_InitialCreateAppUser (Pending)
```
### 4. 移除最後一個 Migration
- 這個尤其好用，因為會根據我們的entity來產生migration，有時候我們entity有寫錯，你不能直接去改，會有快照映射的問題，改了會出問題，用這個讓c#重新產生一次，就可以處理錯誤的問題。
```bash
dotnet ef migrations remove
```

**注意：** 只能移除尚未套用到資料庫的 Migration！
### 5. 回滾到特定 Migration

```bash
dotnet ef database update <MigrationName>
```
**命名建議：**
- 使用描述性名稱（說明這次改了什麼）
- 使用 PascalCase 命名法
- 範例：`AddUserAge`、`RemoveProductCategory`、`UpdateOrderStatus`

> ⚠️ **執行 migration 前必須先停止 API**
>
> `dotnet ef migrations add` 需要重新建構專案，若 `dotnet run` 或 `dotnet watch run` 還在執行中，
> `.exe` 檔會被鎖定，導致建構失敗（`MSB3027: 無法複製檔案，檔案被其他程序鎖定`）。
>
> **正確流程：**
> 1. `Ctrl+C` 停止正在執行的 API
> 2. 執行 `dotnet ef migrations add <名稱>`
> 3. 執行 `dotnet ef database update`
> 4. 再重新 `dotnet run` 或 `dotnet watch run`

---

## 完整工作流程

### 情境 1：第一次建立資料庫

```bash
# 步驟 1：建立 Entity
# 在 Entities/AppUser.cs 中定義你的類別

# 步驟 2：建立 DbContext
# 在 Data/ApplicationDbContext.cs 中設定

# 步驟 3：創建第一個 Migration
dotnet ef migrations add InitialCreate

# 步驟 4：套用到資料庫
dotnet ef database update
```

### 情境 2：修改現有 Entity

```bash
# 步驟 1：修改 Entity 類別
# 例如：在 AppUser 中加入新欄位 Age

# 步驟 2：創建 Migration
dotnet ef migrations add AddUserAge

# 步驟 3：檢查生成的 Migration 檔案
# 確認變更是否正確

# 步驟 4：套用到資料庫
dotnet ef database update
```

### 情境 3：發現 Migration 有錯誤（尚未套用）

```bash
# 移除錯誤的 Migration
dotnet ef migrations remove

# 修正 Entity 或設定

# 重新創建 Migration
dotnet ef migrations add CorrectMigration
```

### 情境 4：已套用的 Migration 需要修正

```bash
# 方法 1：回滾後重新套用
dotnet ef database update PreviousMigration
dotnet ef migrations remove
# 修正後重新創建
dotnet ef migrations add CorrectedMigration
dotnet ef database update

# 方法 2：創建新的 Migration 來修正
dotnet ef migrations add FixPreviousIssue
dotnet ef database update
```

---

## SQLite 查詢（alexcvzz 套件）

> SQLite Viewer 只能看資料（唯讀）；要下 query 用 SQLite (alexcvzz)。

1. `Ctrl+Shift+P` → `SQLite: Open Database` → 選 `.db` 檔
2. 左側 Explorer 最底部出現 **SQLITE EXPLORER**
3. 展開 table → 右鍵 → **New Query (Select)**
4. 會開啟一個 `--SQLite untitled-1` 未儲存檔案
5. 選取要執行的 SQL → 右鍵 **Run Selected Query**（或 `Ctrl+Shift+Q` 全部執行）

---

## Laravel vs .NET 對照

| Laravel | .NET | 說明 |
|---------|------|------|
| Model（`User.php`） | Entity（`AppUser.cs`） | 對應資料表的類別 |
| `$table = 'users'` | `DbSet<AppUser> Users` | 指定操作哪張資料表 |
| Eloquent ORM | Entity Framework Core | ORM 框架 |
| `User::all()` | `context.Users.ToList()` | 取全部資料 |
| `User::find($id)` | `context.Users.Find(id)` | 依 id 取單筆 |

### .NET 的差異

Laravel 的 Model 直接包含資料庫操作邏輯，.NET 拆成兩層：

- **Entity**（`Entities/AppUser.cs`）→ 單純定義資料結構，對應資料表欄位
- **DbContext**（`Data/AppDbContext.cs`）→ 負責資料庫操作，類似 Laravel 的 `DB` facade

---

## JWT Token 驗證
[查看jwt](https://www.jwt.io/)

### 架構說明

JWT 的職責拆成兩個檔案：

| 檔案 | 角色 | 說明 |
|------|------|------|
| `Interfaces/ITokenService.cs` | 介面合約 | 定義「要做什麼」，Controller 依賴介面而非具體實作 |
| `Services/TokenService.cs` | 具體實作 | 實際產生 JWT Token 的邏輯 |

> 使用介面的好處：方便日後替換實作，也方便撰寫單元測試（Mock）。

### 前置需求 — NuGet 套件

```bash
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### 前置需求 — appsettings.json

加入 JWT 金鑰，長度至少 64 字元（太短會拋出 `ArgumentException`）：

```json
"TokenKey": "your-super-secret-key-at-least-64-characters-long-xxxxxxxxxxxx"
```

> ⚠️ 此專案直接用頂層 `"TokenKey"` 讀取（`config["TokenKey"]`），
> 不是 `"JWT:Key"` 巢狀格式，兩種寫法都合法，但要跟 `TokenService.cs` 的讀法一致。

### DI 註冊（Program.cs）

```csharp
// Scoped = 每次 HTTP 請求建立一個新實例，請求結束後銷毀
builder.Services.AddScoped<ITokenService, TokenService>();
```

Controller 只需宣告 `ITokenService`，DI 框架會自動注入 `TokenService`。

### TokenService 運作流程

```
appsettings.json JWT:Key
        ↓
SymmetricSecurityKey（對稱金鑰）
        ↓
Claims（Email + UserId）
        ↓
SigningCredentials（HmacSha512）
        ↓
SecurityTokenDescriptor（有效期 7 天）
        ↓
JwtSecurityTokenHandler.CreateToken()
        ↓
回傳 JWT 字串
```

### 之後啟用 JWT 驗證時，Program.cs 需補上

```csharp
app.UseAuthentication(); // 驗證（誰是你）
app.UseAuthorization();  // 授權（你能做什麼）
```

> ⚠️ 順序不能顛倒，`Authentication` 必須在 `Authorization` 之前。

---

---

## DTO 設計模式（雙 DTO 架構）

認證相關功能使用兩種 DTO，各司其職：

| DTO | 方向 | 用途 |
|-----|------|------|
| `RegisterDTO` | 前端 → 後端（輸入） | 接收使用者填寫的註冊表單資料 |
| `UserDTO` | 後端 → 前端（輸出） | 回傳給前端的使用者資料（含 Token） |

### 為什麼不直接用 AppUser？

```
前端傳入 AppUser  → 有 Mass Assignment 風險（覆蓋 PasswordHash 等欄位）
後端回傳 AppUser  → 洩漏 PasswordHash、PasswordSalt 等敏感欄位
```

正確做法：**輸入用 RegisterDTO，輸出用 UserDTO**。

### UserDTO 設計重點

```csharp
public class UserDTO
{
    public string Id          { get; set; }  // 使用者 UUID
    public string Email       { get; set; }
    public string DisplayName { get; set; }
    public string? ImageUrl   { get; set; }  // nullable：尚未上傳時為 null
    public required string Token { get; set; } // JWT Token（必填）
}
```

前端拿到 `Token` 後，後續請求放在 HTTP Header：
```
Authorization: Bearer <token>
```

---

## AppUser Entity 欄位說明

```csharp
public class AppUser
{
    public string Id           { get; set; }  // UUID 主鍵（自動產生）
    public string DisplayName  { get; set; }  // 顯示名稱（前端展示用）
    public string Email        { get; set; }  // 登入帳號
    public string? ImageUrl    { get; set; }  // 大頭照 URL（可為 null）
    public byte[] PasswordHash { get; set; }  // 密碼雜湊（HMAC-SHA512）
    public byte[] PasswordSalt { get; set; }  // 雜湊用的鹽（HMAC Key）
}
```

> 新增 `ImageUrl` 欄位後需執行 Migration：
> ```bash
> dotnet ef migrations add AddImageUrlToAppUser
> dotnet ef database update
> ```

---

## 完整認證流程

### 註冊（Register）

```
前端 POST /api/account/register
  { DisplayName, Email, Password }
         ↓
後端：
  1. 檢查 Email 是否已存在（UserExists）
  2. new HMACSHA512() → 自動產生隨機 Key 作為鹽
  3. PasswordHash = hmac.ComputeHash(UTF8(Password))
  4. PasswordSalt = hmac.Key
  5. 存入資料庫
  6. 產生 JWT Token（TokenService.CreateToken）
         ↓
回傳 UserDTO（含 Token）
```

### 登入（Login）

```
前端 POST /api/account/login
  { Email, Password }
         ↓
後端：
  1. 用 Email 查詢使用者（SingleOrDefaultAsync）
  2. new HMACSHA512(user.PasswordSalt) → 用儲存的鹽重建 HMAC
  3. computedHash = hmac.ComputeHash(UTF8(輸入密碼))
  4. 逐 byte 比對 computedHash vs user.PasswordHash
  5. 比對通過 → 產生 JWT Token
         ↓
回傳 UserDTO（含 Token）
```

### 為什麼 HMAC 要逐 byte 比對？

```csharp
// ❌ 這樣只比較陣列「參考位址」，永遠不相等
computedHash == user.PasswordHash

// ✅ 正確：逐 byte 比對內容
for (int i = 0; i < computedHash.Length; i++)
{
    if (computedHash[i] != user.PasswordHash[i]) return Unauthorized(...);
}
```

---

## 學習心得

- dotnet 沒有像 Laravel 那樣集中式管理 Route 的地方，通常會寫在 Controller 上
- `WeatherForecastController` 的路由規則：把 class 名稱的 `Controller` 去掉，貼到 URL 上就能看到 API
  - 例如：`WeatherForecastController` → `/WeatherForecast`
- DTO 分「輸入用」和「輸出用」是 API 設計的好習慣，防止敏感欄位洩漏（Security by Design）
- `using var hmac = new HMACSHA512()` 中的 `using` 確保加密物件用完後立即釋放資源


[json to ts](https://transform.tools/json-to-typescript)