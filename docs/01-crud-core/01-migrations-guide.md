# Entity Framework Core Migration 完整指南

> Migration 是 EF Core 管理資料庫結構變更的機制，自動比對 Entity 與上次快照的差異，產生 `Up()` / `Down()` SQL。

## 運作原理

**順序永遠是：先改 Entity → 再跑 migration**，不能反過來。

```
你改 Entity → dotnet ef migrations add → EF Core 比對差異 → 自動產生 Up()/Down()
```

每次 `dotnet ef migrations add` 會產生兩個檔案：

| 檔案 | 用途 | 需要看嗎 |
|------|------|---------|
| `<名稱>.cs` | 實際的 SQL 操作（`Up` 升級、`Down` 回滾） | 要看 |
| `<名稱>.Designer.cs` | EF Core 自用的 model 快照 | 不用管，自動產生 |

---

## 基本指令

### 新增 Migration

```bash
dotnet ef migrations add <MigrationName>
dotnet ef migrations add <MigrationName> -o Data/Migrations  # 指定輸出目錄
```

命名規則：使用 PascalCase 描述變更內容，例如 `AddUserAge`、`RemoveProductCategory`。

### 套用到資料庫

```bash
dotnet ef database update
```

### 查看所有 Migration

```bash
dotnet ef migrations list
```

輸出範例：
```
20251020071659_InitialCreate
20251020090313_InitialCreateAppUser (Pending)
```

### 移除最後一個 Migration（未套用）

```bash
dotnet ef migrations remove
```

Entity 寫錯時很好用：直接刪除 migration，修正 Entity 後重新產生，避免快照映射問題。

> **注意**：只能移除尚未套用的 Migration。

### 回滾到特定 Migration

```bash
dotnet ef database update <MigrationName>
dotnet ef database update 0  # 回滾所有（清空資料庫）
```

### 產生 SQL 腳本（不直接執行）

```bash
dotnet ef migrations script
dotnet ef migrations script Migration1 Migration2     # 指定範圍
dotnet ef migrations script --idempotent -o migrations.sql  # 等冪腳本（可重複執行）
```

### 其他

```bash
dotnet ef                        # 查看所有可用指令
dotnet ef migrations -h          # 查看 migrations 子指令說明
dotnet ef database drop          # 刪除資料庫
```

---

> ⚠️ **執行 migration 前必須先停止 API**
>
> `dotnet ef migrations add` 需要重新建構專案，若 `dotnet run` 或 `dotnet watch run` 仍在執行，
> `.exe` 會被鎖定導致建構失敗（`MSB3027: 無法複製檔案，檔案被其他程序鎖定`）。
>
> 正確流程：`Ctrl+C` → `dotnet ef migrations add <名稱>` → `dotnet ef database update` → `dotnet run`

---

## 工作流程

### 情境 1：第一次建立資料庫

```bash
# 1. 建立 Entity（Entities/AppUser.cs）
# 2. 設定 DbContext（Data/ApplicationDbContext.cs）
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 情境 2：修改現有 Entity

```bash
# 1. 修改 Entity 類別
dotnet ef migrations add AddUserAge
# 2. 確認生成的 migration 檔案內容正確
dotnet ef database update
```

### 情境 3：Migration 有錯誤（尚未套用）

```bash
dotnet ef migrations remove
# 修正 Entity 後
dotnet ef migrations add CorrectMigration
```

### 情境 4：已套用的 Migration 需要修正

```bash
# 方法 1：回滾後重建
dotnet ef database update PreviousMigration
dotnet ef migrations remove
dotnet ef migrations add CorrectedMigration
dotnet ef database update

# 方法 2：新增修正 Migration
dotnet ef migrations add FixPreviousIssue
dotnet ef database update
```

---

## 專案 Migration 紀錄

### AppUser Migration — 2025/10/20

**Entity 定義**（`Entities/AppUser.cs`）：

```csharp
public class AppUser
{
    public int MyProperty { get; set; }
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string DisplayName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
}
```

**Migration 資訊**：

| 項目 | 內容 |
|------|------|
| 名稱 | `InitialCreateAppUser` |
| 建立時間 | 2025-10-20 09:03:13 |
| 檔案 | `Migrations/20251020090313_InitialCreateAppUser.cs` |
| 狀態 | 已套用 |

**Users 資料表結構**：

| 欄位 | 資料類型 | 約束 |
|------|---------|------|
| Id | varchar(255) | PRIMARY KEY（GUID） |
| MyProperty | int | NOT NULL |
| DisplayName | longtext | NOT NULL |
| Email | longtext | NOT NULL |

字元集：utf8mb4

**變更內容**：移除 `Products` 資料表、新增 `Users` 資料表（UTF-8 MB4）

---

## Migration 檔案結構

```csharp
public partial class InitialCreateAppUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<string>(type: "varchar(255)", nullable: false),
                MyProperty = table.Column<int>(type: "int", nullable: false),
                DisplayName = table.Column<string>(type: "longtext", nullable: false),
                Email = table.Column<string>(type: "longtext", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

檔案命名格式：`{yyyyMMdd}{HHmmss}_{MigrationName}.cs`

---

## 進階技巧

### 自訂資料表名稱

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AppUser>().ToTable("tbl_users");
}
```

### 設定欄位屬性

```csharp
public class AppUser
{
    [Key]
    public string Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; }

    [EmailAddress]
    [Column(TypeName = "varchar(255)")]
    public string Email { get; set; }

    [Column("user_age")]
    public int Age { get; set; }
}
```

### 手動修改 Migration（加入初始資料或索引）

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(...);

    migrationBuilder.InsertData(
        table: "Users",
        columns: new[] { "Id", "DisplayName", "Email" },
        values: new object[] { "1", "Admin", "admin@example.com" }
    );

    migrationBuilder.CreateIndex(
        name: "IX_Users_Email",
        table: "Users",
        column: "Email",
        unique: true
    );
}
```

### 資料遷移（新增 NOT NULL 欄位）

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 先允許 NULL
    migrationBuilder.AddColumn<string>(name: "PhoneNumber", table: "Users", nullable: true);

    // 填充預設值
    migrationBuilder.Sql("UPDATE Users SET PhoneNumber = '0000000000' WHERE PhoneNumber IS NULL");

    // 改為 NOT NULL
    migrationBuilder.AlterColumn<string>(name: "PhoneNumber", table: "Users", nullable: false);
}
```

---

## 常見問題

### 找不到 dotnet-ef 命令

```bash
dotnet tool install --global dotnet-ef
```

### Migration 已套用但想修改

```bash
# 方法 1：回滾後重建
dotnet ef database update 0
dotnet ef migrations remove

# 方法 2：新增修正 Migration
dotnet ef migrations add FixUserTable
dotnet ef database update
```

### 多個 DbContext

```bash
dotnet ef migrations add MigrationName --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

### 連線字串錯誤

檢查項目：
- `appsettings.json` 連線字串是否正確
- 資料庫服務是否運行
- 帳號密碼是否正確

### Migration 衝突（團隊協作）

```bash
git pull
dotnet ef migrations list
dotnet ef database update
# 若有衝突
dotnet ef migrations remove
dotnet ef migrations add YourMigrationName
```

---

## 生產環境部署

```bash
# 產生等冪 SQL 腳本（可重複執行，適合 CI/CD）
dotnet ef migrations script --idempotent -o migrations.sql
```

```yaml
# GitHub Actions
- name: Apply Migrations
  run: dotnet ef database update --project API/API.csproj
```

```bash
# 使用環境變數覆蓋連線字串
export ConnectionStrings__DefaultConnection="your-production-connection"
dotnet ef database update
```

---

## 最佳實踐

- 每次 Entity 變更都建立 Migration，使用描述性名稱（`AddUserPhoneNumber` 而非 `Fix`）
- Migration 檔案加入 Git（`git add Migrations/`）
- 套用前確認 `Up()` / `Down()` 內容正確
- 生產環境用 SQL 腳本，不直接執行 `database update`
- 不要手動修改資料庫欄位，不要刪除已套用的 Migration 檔案

---

## 參考資源

- [EF Core Migrations 官方文件](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [EF Core CLI 工具](https://docs.microsoft.com/ef/core/cli/dotnet)
- [資料庫提供者文件](https://docs.microsoft.com/ef/core/providers/)
