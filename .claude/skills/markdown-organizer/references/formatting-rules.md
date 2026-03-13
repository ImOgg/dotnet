# Markdown 格式規範

## 標題（Headings）

```
✅ 正確
# H1 — 文件主標題（每篇只有一個）
## H2 — 主要章節
### H3 — 子章節
#### H4 — 細項（謹慎使用，避免層級過深）

❌ 錯誤
#沒有空格
## 使用過多層級的 ##### H5
```

規則：
- `#` 符號後必須有一個空格
- 標題前後各留一個空行（文件開頭的 H1 除外）
- 不跳級（H1 → H3 是錯的，應為 H1 → H2 → H3）

---

## 列表（Lists）

```
✅ 正確（統一使用 - ）
- 項目一
- 項目二
  - 子項目（縮排 2 個空格）

✅ 有序列表
1. 第一步
2. 第二步
3. 第三步

❌ 錯誤
* 混用星號
+ 加號
- 和連字號混用
```

規則：
- 無序列表統一使用 `-`
- 子項目縮排 2 個空格
- 列表前後各留一個空行

---

## 代碼塊（Code Blocks）

````
✅ 正確 — 必須標注語言
```bash
dotnet run
```

```csharp
var result = await _context.Users.ToListAsync();
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

```sql
SELECT * FROM Users WHERE Id = 1;
```

❌ 錯誤 — 沒有語言標注
```
dotnet run
```
````

常用語言標籤：`bash`、`csharp`、`json`、`yaml`、`sql`、`xml`、`typescript`、`javascript`

行內代碼：使用反引號 `` `ClassName` ``、`` `methodName()` ``、`` `dotnet run` ``

---

## 表格（Tables）

```markdown
✅ 正確 — 對齊且有空格
| 欄位 | 說明 | 範例 |
|------|------|------|
| Id   | 主鍵 | 1    |
| Name | 名稱 | John |

✅ 對齊方式
| 左對齊 | 置中 | 右對齊 |
|:-------|:----:|-------:|
| 文字   |  文字 |   文字 |
```

規則：
- 每個欄位前後各有一個空格
- 分隔線 `---` 至少 3 個連字號
- 表格前後各留一個空行

---

## 強調與引用

```markdown
✅ 粗體（重要詞彙）
**重要概念** 或 **`重要代碼`**

✅ 斜體（術語或書名）
*Entity Framework*

✅ 引用塊（注意事項）
> **注意**：這是需要特別留意的內容。

> **提示**：這是有用的技巧。

> **警告**：這個操作可能造成資料遺失。
```

---

## 中英文排版

```
✅ 正確 — 中英文之間加空格
使用 ASP.NET Core 開發 Web API
執行 `dotnet run` 指令

❌ 錯誤 — 缺少空格
使用ASP.NET Core開發Web API
執行`dotnet run`指令
```

規則：
- 中文與英文之間加一個空格
- 中文與數字之間加一個空格
- 中文與行內代碼之間加一個空格
- 全形標點（。，、：）不需要加空格

---

## 連結

```markdown
✅ 外部連結
[官方文件](https://docs.microsoft.com/...)

✅ 文件內錨點
[前往章節](#章節名稱)

✅ 相對路徑（同目錄）
[參考文件](./other-doc.md)

✅ 參考式連結（連結較多時）
請參考 [EF Core 文件][ef-core]

[ef-core]: https://docs.microsoft.com/ef/core
```
