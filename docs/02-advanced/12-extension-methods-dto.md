# C# 擴充方法（Extension Methods）與 Entity → DTO 轉換

## 什麼是擴充方法？

擴充方法讓你可以「為既有的類別新增方法」，而不需要修改原始類別或繼承它。

### 語法規則

```csharp
// 必須是 public static class
public static class AppUserExtensions
{
    // 第一個參數加上 this 關鍵字，代表「被擴充的型別」
    public static UserDTO ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDTO { ... };
    }
}
```

呼叫時就像呼叫 `AppUser` 本身的方法一樣：

```csharp
// 不用擴充方法（原本的寫法）
return new UserDTO
{
    Id = user.Id,
    Email = user.Email,
    DisplayName = user.DisplayName,
    ImageUrl = user.ImageUrl,
    Token = tokenService.CreateToken(user)
};

// 使用擴充方法（重構後）
return user.ToDto(tokenService);
```

---

## 本專案的實際範例

### 擴充方法定義

[AppUserExtensions.cs](../../API/Extemsions/AppUserExtensions.cs)

```csharp
using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Extemsions;

public static class AppUserExtensions
{
    public static UserDTO ToDto(this AppUser user, ITokenService tokenService)
    {
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            ImageUrl = user.ImageUrl,
            Token = tokenService.CreateToken(user)
        };
    }
}
```

### 在 Controller 使用

[AccountController.cs](../../API/Controllers/AccountController.cs) 的 `Register` 和 `Login` 都使用同一個擴充方法：

```csharp
// Register
context.Users.Add(user);
await context.SaveChangesAsync();
return user.ToDto(tokenService);   // ✅

// Login
if (驗證密碼失敗) return Unauthorized("Invalid password");
return user.ToDto(tokenService);   // ✅
```

---

## 為什麼用擴充方法做 Entity → DTO 轉換？

| 比較 | 手動在 Controller 建立 DTO | 擴充方法 |
|------|--------------------------|---------|
| 重複程式碼 | 每個 action 都要寫一遍 | 一處定義，到處使用 |
| 維護 | 改欄位要改多個地方 | 只改擴充方法 |
| 可讀性 | 業務邏輯被 mapping 程式碼淹沒 | Controller 邏輯一目了然 |

---

## 注意事項

### 命名空間要正確 import

擴充方法所在的 namespace 必須在呼叫的地方 `using`：

```csharp
// AccountController.cs
using API.Extemsions;   // ← 少了這行就找不到 ToDto()
```

### 型別名稱要一致

方法簽名的回傳型別必須與實際 DTO 類別名稱一致：

```csharp
// ❌ 錯誤：UserDto（小寫 d）與類別 UserDTO 不同
public static UserDto ToDto(this AppUser user, ...)

// ✅ 正確
public static UserDTO ToDto(this AppUser user, ...)
```

### 不要重複 using

```csharp
// ❌
using API.Interfaces;
using API.Interfaces;   // 多餘，編譯器會警告

// ✅
using API.Interfaces;
```

---

## 適合用擴充方法的場景

- Entity → DTO 轉換（本例）
- `IServiceCollection` 服務註冊整理（`AddApplicationServices()`）
- 字串工具（`Truncate()`、`ToTitleCase()`）
- 通用的 null 檢查、格式化等 utility 邏輯
