# EF Core `.Include()` — 載入關聯資料

> `.Include()` 是 EF Core 的「一起載入」指令，告訴 EF Core 在查詢時順便 JOIN 關聯的資料表，避免導覽屬性回傳 `null`。

## 問題：導覽屬性預設是 null

EF Core 的導覽屬性（Navigation Property）預設**不會自動載入**。

```csharp
public class MemberLike
{
    public string SourceMemberId { get; set; }
    public Member SourceMember { get; set; }  // 導覽屬性
    public string TargetMemberId { get; set; }
    public Member TargetMember { get; set; }  // 導覽屬性
}
```

不加 `.Include()` 查出來的結果：

```json
{
  "sourceMemberId": "davis-id",
  "sourceMember": null,       // ← null，因為沒有 JOIN
  "targetMemberId": "karen-id",
  "targetMember": null        // ← null，因為沒有 JOIN
}
```

## 解法：加上 `.Include()`

```csharp
// 沒有 Include — 只撈 Likes 表
query.Where(like => like.SourceMemberId == memberId)
// SQL: SELECT * FROM Likes WHERE SourceMemberId = @id

// 有 Include — 同時 JOIN Members 表
query.Where(like => like.SourceMemberId == memberId)
     .Include(like => like.TargetMember)
// SQL: SELECT * FROM Likes
//      JOIN Members ON Likes.TargetMemberId = Members.Id
//      WHERE SourceMemberId = @id
```

加上 `.Include()` 後，`TargetMember` 就會有完整資料。

## 實際範例：LikesRepository

根據 `predicate` 不同，需要 include 不同的關聯：

```csharp
query = likesParams.Predicate switch
{
    // 我按讚別人 → 想看「被我按讚的那個人」→ Include TargetMember
    "liked" => query
        .Where(like => like.SourceMemberId == likesParams.MemberId)
        .Include(like => like.TargetMember),

    // 別人按讚我 → 想看「按讚我的那個人」→ Include SourceMember
    "likedBy" => query
        .Where(like => like.TargetMemberId == likesParams.MemberId)
        .Include(like => like.SourceMember),

    // 互讚 → 想看「對方」→ Include SourceMember
    _ => query
        .Where(...)
        .Include(like => like.SourceMember)
};
```

## 多層關聯：ThenInclude

若要載入「關聯的關聯」，使用 `.ThenInclude()`：

```csharp
query.Include(like => like.TargetMember)
         .ThenInclude(member => member.Photos)  // 再 JOIN Photos 表
```

## 與 Laravel 的對比

EF Core 的 `.Include()` 和 Laravel 的 `with()` 是完全相同的概念，都叫做 **Eager Loading（預先載入）**。

| Laravel | EF Core |
|---|---|
| `with('targetMember')` | `.Include(x => x.TargetMember)` |
| `with('targetMember.photos')` | `.Include(x => x.TargetMember).ThenInclude(x => x.Photos)` |

兩者都是為了解決 **N+1 查詢問題**：若不預先載入，每筆資料要再發一次 SQL 查詢取關聯，100 筆就發 101 次查詢，`.Include()` / `with()` 讓它一次 JOIN 搞定。

## 重點整理

| | 說明 |
|---|---|
| `.Include()` | JOIN 一層關聯資料表（同 Laravel `with()`） |
| `.ThenInclude()` | 在 Include 的基礎上再 JOIN 下一層（同 Laravel `with('a.b')`） |
| 不加 Include | 導覽屬性為 `null` |
| 等同 SQL | `JOIN` |
