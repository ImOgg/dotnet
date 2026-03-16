# Postman JWT 401 除錯筆記

## 問題描述

登入拿到 JWT Token 後，打需要驗證的 API 端點仍然回傳 **401 Unauthorized**。

---

## 根本原因：Postman Authorization 設定衝突

Postman 有兩個地方可以設定認證：

| 位置 | 說明 |
|------|------|
| **Authorization 分頁** | Postman 內建的 Auth 設定，會自動產生對應 header |
| **Headers 分頁** | 手動新增 HTTP header |

**問題**：如果 Authorization 分頁設定了任何 Auth Type（例如 Basic Auth），它產生的 header 會**覆蓋**你在 Headers 分頁手動填的 `Authorization` header。

### 實際案例

Authorization 分頁設成 Basic Auth（空帳號密碼）時，實際送出的是：

```
Authorization: Basic Og==
```

`Basic Og==` = Base64(`:`）= 空的 username:password，根本不是 Bearer Token。

---

## 正確做法

### 方法一（推薦）：用 Authorization 分頁的 Bearer Token

1. 點 request 的 **Authorization** 分頁
2. Type 選 **Bearer Token**
3. Token 欄位貼入 JWT（**不需要加 "Bearer " 前綴**，Postman 自動加）

### 方法二：手動加 Header

1. Authorization 分頁 Type 設成 **No Auth**
2. **Headers** 分頁手動新增：
   - KEY: `Authorization`
   - VALUE: `Bearer eyJhbGciOiJIUzUx...`（**"Bearer " 後面有空格**）

> 兩種方法選一種，**不要混用**。

---

## 除錯技巧：用 Postman Console 確認實際送出的 header

按 `Ctrl + Alt + C` 開啟 Postman Console，送出 request 後展開查看 **Request Headers**，確認 `Authorization` 欄位的值是否正確。

若看到 `Authorization: Basic Og==` → Authorization 分頁設定覆蓋了 Bearer Token。

---

## 相關的 Server 端 Bug

同一次 debug 發現：`GetMemberByIdAsync` 用 `FindAsync` 不支援 `Include`，導致 `member.User` 為 null，存取時拋出 NullReferenceException（500）。

**修法**：改用 `SingleOrDefaultAsync` + `Include(x => x.User)`。

```csharp
// Before（User 不會被載入）
return await context.Members.FindAsync(id);

// After
return await context.Members
    .Include(x => x.User)
    .SingleOrDefaultAsync(x => x.Id == id);
```
