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

前端拿到 `Token` 後，後續請求放在 HTTP Header：
```
Authorization: Bearer <token>
```