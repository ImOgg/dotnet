# Task vs void

> 核心差別：有沒有需要**等待**的 I/O 操作。

## 快速比較

| | `void` | `Task` |
|--|--------|--------|
| 執行方式 | 同步 | 非同步 |
| 可否 `await` | 否 | 是 |
| 適用情境 | 純記憶體操作 | DB、API、檔案讀寫 |

## 判斷邏輯

```
有沒有碰到 I/O？
    ├── 有（查 DB、呼叫 API、讀寫檔案）→ Task（非同步）
    └── 沒有（純記憶體操作、計算）    → void（同步）
```

## 實際例子（LikesRepository）

```csharp
// void → 只是告訴 EF Core「追蹤這個物件」，不碰 DB，瞬間完成
public void AddLike(MemberLike like) { ... }
public void DeleteLike(MemberLike like) { ... }

// Task → 真的把資料寫進 MySQL，需要等待網路 I/O
public Task<bool> SaveAllChanges() { ... }

// Task → 要去 DB 查詢，需要等待
public Task<MemberLike> GetUserLike(string sourceMemberId, string targetMemberId) { ... }
```

## 生活比喻

```
void   = 把東西放進購物車（本機操作，瞬間完成）
Task   = 結帳送出訂單（要等伺服器回應，不知道多久）
```

## Task 的回傳值

- `Task` — 非同步，但不回傳值（等同 async 的 void）
- `Task<bool>` — 非同步，回傳 bool
- `Task<MemberLike>` — 非同步，回傳 MemberLike 物件
