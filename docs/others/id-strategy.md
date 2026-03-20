# Entity ID 類型選擇：UUID vs int

> 決定用 `string`（UUID）還是 `int` 當主鍵，核心考量是**這個 Id 會不會暴露給外部**以及**安全性需求**。

## 判斷邏輯

```
這個 Id 會暴露給外部嗎？
    ├── 會（JWT、URL、API Response）
    │       ↓
    │   有安全疑慮嗎？（可以被猜測到其他資源嗎？）
    │       ├── 有 → UUID (string)
    │       └── 沒有 → 看情況，int 也可以
    │
    └── 不會（純 DB 內部 JOIN 用）→ int 就夠了
```

## 各 Entity 對應

| Entity | ID 類型 | 原因 |
|--------|---------|------|
| `AppUser` | `string` (UUID) | Id 會放進 JWT，需防猜測 |
| `Member` | `string` (UUID) | Shared PK，共用 AppUser.Id |
| `Photo` | `int` | 純內部，不對外暴露 |
| `MemberLike` | 無 PK（複合 PK） | 用兩個 FK 組成 |

## UUID 適合的情境

- 使用者帳號（攻擊者無法從 id=1, 2, 3 猜測其他使用者）
- 訂單（不想讓外部從流水號推算業務量）
- 付款紀錄、私人文件

## int 適合的情境

- 照片、標籤、分類等「知道 id=5 也沒有安全問題」的資源
- 純粹內部 JOIN 用的關聯表

## 進階：兩者並用

同時有 `int Id`（DB PK，效能好）+ `Guid PublicId`（對外暴露，安全），兼顧效能與安全，但複雜度較高，視需求決定。

```csharp
public int Id { get; set; }           // DB 內部 JOIN 用
public Guid PublicId { get; set; }    // 對外 API 暴露用
```