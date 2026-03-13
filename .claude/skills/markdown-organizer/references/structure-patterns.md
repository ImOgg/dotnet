# 筆記結構模式

## 模式 A：概念說明型

適用於：解釋一個技術概念或功能（例如：「什麼是 Middleware」、「EF Core 關係」）

```markdown
# 概念名稱

> 一句話定義：XXX 是用來做什麼的。

## 為什麼需要它

說明使用場景和解決的問題。

## 核心概念

### 概念一
說明...

### 概念二
說明...

## 基本使用

```csharp
// 最簡單的使用範例
```

## 常見模式

| 場景 | 做法 |
|------|------|
| 情境一 | 方法一 |
| 情境二 | 方法二 |

## 注意事項

- 留意...
- 避免...

## 參考資源

- [官方文件](https://...)
```

---

## 模式 B：操作步驟型

適用於：記錄一系列操作步驟（例如：「建立 Migration」、「部署到 Docker」）

```markdown
# 操作名稱

> 摘要：完成此操作的目的和結果。

## 前置條件

- [ ] 已安裝 XXX
- [ ] 已設定 XXX

## 步驟

### 步驟 1：建立 XXX

```bash
# 執行指令
dotnet ef migrations add InitialCreate
```

說明這個步驟的作用。

### 步驟 2：更新 XXX

```bash
dotnet ef database update
```

### 步驟 3：驗證結果

確認 XXX 已正確建立...

## 常見錯誤

### 錯誤：XXX not found

**原因**：...
**解決**：...

## 指令速查

```bash
# 新增 Migration
dotnet ef migrations add <名稱>

# 套用到資料庫
dotnet ef database update

# 列出所有 Migration
dotnet ef migrations list

# 移除最後一個 Migration
dotnet ef migrations remove
```
```

---

## 模式 C：API 參考型

適用於：記錄 API endpoint 或 class 的用法（例如：「Controller 參考」）

```markdown
# API / Class 名稱

> 說明這個 API 或 class 的用途。

## 基本設定

```csharp
// 注入或初始化方式
```

## 方法列表

| 方法 | 參數 | 回傳 | 說明 |
|------|------|------|------|
| `MethodA()` | `string id` | `Task<T>` | 說明 |
| `MethodB()` | - | `IEnumerable<T>` | 說明 |

## 範例

### 範例 1：XXX 情境

```csharp
// 完整使用範例
[HttpGet("{id}")]
public async Task<ActionResult<AppUser>> GetUser(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null) return NotFound();
    return user;
}
```

### 範例 2：XXX 情境

```csharp
// 另一個使用情境
```

## 相關資源

- 相關筆記：[XXX](./xxx.md)
```

---

## 模式 D：比較對照型

適用於：比較兩種以上的做法（例如：「同步 vs 非同步」、「各種驗證方式比較」）

```markdown
# 比較：A vs B

> 摘要：說明何時選擇 A，何時選擇 B。

## 快速比較

| 特性 | A | B |
|------|---|---|
| 效能 | 高 | 中 |
| 複雜度 | 低 | 高 |
| 適用場景 | XXX | XXX |

## 方式 A：名稱

**優點**：
- 優點一
- 優點二

**缺點**：
- 缺點一

```csharp
// A 的範例代碼
```

## 方式 B：名稱

**優點**：
- 優點一

**缺點**：
- 缺點一
- 缺點二

```csharp
// B 的範例代碼
```

## 選擇建議

- 如果 XXX → 使用 A
- 如果 XXX → 使用 B
```

---

## 整理現有筆記的決策流程

```
讀取筆記
    ↓
判斷類型
    ├── 解釋概念？→ 套用模式 A
    ├── 操作步驟？→ 套用模式 B
    ├── API 參考？→ 套用模式 C
    └── 比較做法？→ 套用模式 D
    ↓
套用格式規範（見 formatting-rules.md）
    ↓
補充缺少的區塊（例如：沒有摘要、沒有範例代碼）
    ↓
輸出整理後的版本，並列出修改摘要
```
