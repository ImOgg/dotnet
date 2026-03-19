# 比較：FirstOrDefault vs SingleOrDefault

> 兩者都是 LINQ 查詢找不到時回傳 `null`，差別在於**找到多筆時的行為**。

## 快速比較

| 情境 | `FirstOrDefault` | `SingleOrDefault` |
|------|:----------------:|:-----------------:|
| 找到 0 筆 | 回傳 `null` | 回傳 `null` |
| 找到 1 筆 | 回傳那筆 | 回傳那筆 |
| 找到 2+ 筆 | 回傳第一筆，不報錯 | 拋出 `InvalidOperationException` |
| 效能 | 找到即停止 | 掃完整個集合 |

## FirstOrDefault

適用於：預期有多筆、或查詢條件是主鍵（唯一但不需要強制驗證）。

```csharp
// Id 是主鍵，找到就停，效能較好
var photo = member.Photos.FirstOrDefault(p => p.Id == photoId);
```

## SingleOrDefault

適用於：預期最多一筆，多筆代表資料異常，需要強制驗證唯一性。

```csharp
// Email 理應唯一，多筆就是資料錯誤
var user = await context.Users.SingleOrDefaultAsync(u => u.Email == email);
```

## 選擇建議

- 查詢**主鍵** → 用 `FirstOrDefault`（效能好，主鍵本來就唯一）
- 查詢**業務唯一欄位**（Email、身分證字號）→ 用 `SingleOrDefault`（多筆要報錯）
- 查詢**一般條件**可能有多筆 → 用 `FirstOrDefault`
