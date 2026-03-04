# Entity Framework Core Migration 完整指南

## 什麼是 Migration？

**Migration（遷移）** 是 Entity Framework Core 用來管理資料庫結構變更的機制。

### 為什麼需要 Migration？

```
沒有 Migration 的情況：
開發者修改 Entity → 手動寫 SQL → 手動執行 → 容易出錯

使用 Migration 的情況：
開發者修改 Entity → EF Core 自動產生 SQL → 版本控制 → 安全可靠
```

### Migration 的優點

- ✅ **版本控制**：每次資料庫變更都有紀錄
- ✅ **可回滾**：可以退回到之前的版本
- ✅ **團隊協作**：Migration 檔案可以加入 Git
- ✅ **自動化**：不需手寫 SQL
- ✅ **跨環境部署**：開發、測試、生產環境一致

---

## 基本命令

### 1. 創建 Migration

```bash
dotnet ef migrations add <MigrationName>
```

**範例：**
```bash
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddUserEmail
dotnet ef migrations add UpdateProductPrice
```

**命名建議：**
- 使用描述性名稱（說明這次改了什麼）
- 使用 PascalCase 命名法
- 範例：`AddUserAge`、`RemoveProductCategory`、`UpdateOrderStatus`

### 2. 套用 Migration 到資料庫

```bash
dotnet ef database update
```

這會將所有尚未套用的 Migration 執行到資料庫。

### 3. 查看所有 Migrations

```bash
dotnet ef migrations list
```

**輸出範例：**
```
20251020071659_InitialCreate
20251020090313_InitialCreateAppUser (Pending)
```

### 4. 移除最後一個 Migration

```bash
dotnet ef migrations remove
```

**注意：** 只能移除尚未套用到資料庫的 Migration！

### 5. 回滾到特定 Migration

```bash
dotnet ef database update <MigrationName>
```

**範例：**
```bash
# 回滾到 InitialCreate
dotnet ef database update InitialCreate

# 回滾所有 Migration（清空資料庫）
dotnet ef database update 0
```

### 6. 產生 SQL 腳本（不直接執行）

```bash
dotnet ef migrations script
```

**進階用法：**
```bash
# 產生從 Migration1 到 Migration2 的 SQL
dotnet ef migrations script Migration1 Migration2

# 產生所有 Migration 的 SQL
dotnet ef migrations script -o migrations.sql

# 產生等冪腳本（可重複執行）
dotnet ef migrations script --idempotent
```

---

## 完整工作流程

### 情境 1：第一次建立資料庫

```bash
# 步驟 1：建立 Entity
# 在 Entities/AppUser.cs 中定義你的類別

# 步驟 2：建立 DbContext
# 在 Data/ApplicationDbContext.cs 中設定

# 步驟 3：創建第一個 Migration
dotnet ef migrations add InitialCreate

# 步驟 4：套用到資料庫
dotnet ef database update
```

### 情境 2：修改現有 Entity

```bash
# 步驟 1：修改 Entity 類別
# 例如：在 AppUser 中加入新欄位 Age

# 步驟 2：創建 Migration
dotnet ef migrations add AddUserAge

# 步驟 3：檢查生成的 Migration 檔案
# 確認變更是否正確

# 步驟 4：套用到資料庫
dotnet ef database update
```

### 情境 3：發現 Migration 有錯誤（尚未套用）

```bash
# 移除錯誤的 Migration
dotnet ef migrations remove

# 修正 Entity 或設定

# 重新創建 Migration
dotnet ef migrations add CorrectMigration
```

### 情境 4：已套用的 Migration 需要修正

```bash
# 方法 1：回滾後重新套用
dotnet ef database update PreviousMigration
dotnet ef migrations remove
# 修正後重新創建
dotnet ef migrations add CorrectedMigration
dotnet ef database update

# 方法 2：創建新的 Migration 來修正
dotnet ef migrations add FixPreviousIssue
dotnet ef database update
```

---

## 專案 Migration 紀錄

### AppUser Migration - 2025/10/20

#### Entity 定義
位置：`Entities/AppUser.cs`

```csharp
public class AppUser
{
    public int MyProperty { get; set; }
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string DisplayName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
}
```

#### Migration 資訊

- **Migration 名稱：** `InitialCreateAppUser`
- **建立時間：** 2025-10-20 09:03:13
- **檔案位置：** `Migrations/20251020090313_InitialCreateAppUser.cs`
- **狀態：** ✅ 已套用

#### 執行命令

```bash
# 創建 Migration
dotnet ef migrations add InitialCreateAppUser

# 套用到資料庫
dotnet ef database update
```

#### 資料表結構

**Users 資料表：**
| 欄位名稱 | 資料類型 | 約束 | 說明 |
|---------|---------|------|------|
| Id | varchar(255) | PRIMARY KEY | 使用 GUID |
| MyProperty | int | NOT NULL | 整數屬性 |
| DisplayName | longtext | NOT NULL | 顯示名稱 |
| Email | longtext | NOT NULL | Email 地址 |

**字元集：** utf8mb4

#### 變更內容
- ✅ 移除了 `Products` 資料表
- ✅ 新增 `Users` 資料表
- ✅ 使用 UTF-8 MB4 字元集

#### 執行結果

```
✅ Build succeeded
✅ Migration created successfully
✅ Database updated successfully
✅ Table 'Users' created
```

---

## Migration 檔案結構

### Migration 檔案內容

```csharp
public partial class InitialCreateAppUser : Migration
{
    // 套用 Migration 時執行
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

    // 回滾 Migration 時執行
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

### 檔案命名規則

```
格式：{Timestamp}_{MigrationName}.cs

範例：
20251020090313_InitialCreateAppUser.cs
│       │      │
│       │      └─ Migration 名稱
│       └──────── 時間（HHmmss）
└──────────────── 日期（yyyyMMdd）
```

---

## 進階技巧

### 1. 自訂資料表名稱

```csharp
// ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AppUser>()
        .ToTable("tbl_users");  // 自訂資料表名稱
}
```

### 2. 設定欄位屬性

```csharp
public class AppUser
{
    [Key]
    public string Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; }

    [EmailAddress]
    [Column(TypeName = "varchar(255)")]  // 指定資料庫類型
    public string Email { get; set; }

    [Column("user_age")]  // 自訂欄位名稱
    public int Age { get; set; }
}
```

### 3. 手動修改 Migration

有時需要手動修改生成的 Migration：

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // EF Core 自動生成的
    migrationBuilder.CreateTable(...);

    // 手動加入的初始資料
    migrationBuilder.InsertData(
        table: "Users",
        columns: new[] { "Id", "DisplayName", "Email" },
        values: new object[] { "1", "Admin", "admin@example.com" }
    );

    // 手動加入的索引
    migrationBuilder.CreateIndex(
        name: "IX_Users_Email",
        table: "Users",
        column: "Email",
        unique: true
    );
}
```

### 4. 資料遷移（Data Migration）

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 加入新欄位（允許 NULL）
    migrationBuilder.AddColumn<string>(
        name: "PhoneNumber",
        table: "Users",
        nullable: true);

    // 填充現有資料的預設值
    migrationBuilder.Sql(
        "UPDATE Users SET PhoneNumber = '0000000000' WHERE PhoneNumber IS NULL");

    // 改為 NOT NULL
    migrationBuilder.AlterColumn<string>(
        name: "PhoneNumber",
        table: "Users",
        nullable: false);
}
```

---

## 常見問題與解決方案

### 問題 1：找不到 dotnet-ef 命令

**錯誤訊息：**
```
'dotnet-ef' is not recognized as an internal or external command
```

**解決方法：**
```bash
dotnet tool install --global dotnet-ef
```

### 問題 2：Migration 已套用，但想修改

**錯誤訊息：**
```
The migration '20251020090313_InitialCreateAppUser' has already been applied to the database.
```

**解決方法：**
```bash
# 方法 1：回滾後重建
dotnet ef database update 0  # 清空所有
dotnet ef migrations remove  # 移除 Migration
# 修改後重新創建

# 方法 2：創建新的修正 Migration
dotnet ef migrations add FixUserTable
dotnet ef database update
```

### 問題 3：多個 DbContext

**錯誤訊息：**
```
More than one DbContext was found.
```

**解決方法：**
```bash
dotnet ef migrations add MigrationName --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext
```

### 問題 4：連線字串錯誤

**錯誤訊息：**
```
Unable to connect to database
```

**檢查項目：**
1. 確認 `appsettings.json` 中的連線字串正確
2. 確認資料庫服務正在運行
3. 確認帳號密碼正確

### 問題 5：Migration 衝突（團隊協作）

**情境：** 多人同時創建 Migration

**解決方法：**
```bash
# 1. 拉取最新的程式碼
git pull

# 2. 檢查 Migration 狀態
dotnet ef migrations list

# 3. 套用其他人的 Migration
dotnet ef database update

# 4. 如有衝突，可能需要重建自己的 Migration
dotnet ef migrations remove
dotnet ef migrations add YourMigrationName
```

---

## 生產環境部署建議

### 1. 產生 SQL 腳本

```bash
# 產生等冪腳本（可重複執行）
dotnet ef migrations script --idempotent -o migrations.sql
```

### 2. 在 CI/CD 中自動套用

```yaml
# GitHub Actions 範例
- name: Apply Migrations
  run: dotnet ef database update --project API/API.csproj
```

### 3. 使用環境變數

```bash
# 使用不同環境的連線字串
export ConnectionStrings__DefaultConnection="your-production-connection"
dotnet ef database update
```

---

## 最佳實踐

### ✅ 應該做的

1. **每次變更都創建 Migration**
   ```bash
   dotnet ef migrations add DescriptiveName
   ```

2. **加入版本控制**
   ```bash
   git add Migrations/
   git commit -m "Add user table migration"
   ```

3. **檢查 Migration 內容**
   - 確認 Up() 和 Down() 方法正確
   - 確認沒有刪除重要資料

4. **使用描述性名稱**
   - ✅ `AddUserPhoneNumber`
   - ❌ `Update1`、`Fix`

5. **在開發環境測試**
   ```bash
   # 測試套用
   dotnet ef database update
   # 測試回滾
   dotnet ef database update PreviousMigration
   ```

### ❌ 不應該做的

1. **不要直接修改資料庫**
   - ❌ 手動在資料庫加欄位
   - ✅ 透過 Migration 管理

2. **不要刪除已套用的 Migration 檔案**
   - 會導致 Migration 歷史紀錄混亂

3. **不要在生產環境直接執行 `database update`**
   - 使用 SQL 腳本
   - 在維護時段執行

4. **不要忽略 Migration 衝突**
   - 必須解決後才能繼續

---

## 參考資源

- [EF Core Migrations 官方文件](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [EF Core CLI 工具](https://docs.microsoft.com/ef/core/cli/dotnet)
- [資料庫提供者文件](https://docs.microsoft.com/ef/core/providers/)
