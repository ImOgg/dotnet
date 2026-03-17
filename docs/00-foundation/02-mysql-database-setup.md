# MySQL 資料庫設定

> 在 ASP.NET Core 專案中整合 MySQL，使用 Pomelo EF Core Provider 完成套件安裝、DbContext 設定到資料庫遷移的完整流程。

## 安裝套件

```bash
# Pomelo MySQL Provider
dotnet add package Pomelo.EntityFrameworkCore.MySql

# EF Core Design 工具（用於資料庫遷移）
dotnet add package Microsoft.EntityFrameworkCore.Design

# 全域 EF Core CLI 工具
dotnet tool install --global dotnet-ef
```

## 建立 DbContext

`API/Data/ApplicationDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
```

## 建立實體類別

`API/Models/Product.cs`

```csharp
namespace API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

## 設定連接字串

`API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=MyDatabase;User=root;Password=your_password;"
  }
}
```

> **注意**：將 `your_password` 替換成實際的 MySQL 密碼。

## 註冊 DbContext

`API/Program.cs`

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 21))
    )
);
```

> **提示**：使用 `MySqlServerVersion` 明確指定版本，而非 `ServerVersion.AutoDetect()`，可避免設計時期連接資料庫的問題。

## 資料庫遷移

執行前確認：

- MySQL Server 已安裝並啟動
- `appsettings.json` 連接字串正確（密碼、資料庫名稱）
- MySQL 用戶具有建立資料庫的權限

```bash
# 從 API/ 目錄執行
cd API

# 建立遷移
dotnet ef migrations add InitialCreate

# 套用遷移
dotnet ef database update
```

成功後，遷移檔案會產生在 `API/Migrations/`。

## 驗證連接

```bash
dotnet run
```

## 常見問題

### Access denied for user

檢查 `appsettings.json` 中的帳號密碼是否正確。

### Unknown database

`dotnet ef database update` 會自動建立資料庫。若仍失敗，手動建立：

```sql
CREATE DATABASE MyDatabase;
```

### 確認 MySQL 版本

```sql
SELECT VERSION();
```

若版本不是 `8.0.21`，請同步更新 `Program.cs` 中的 `MySqlServerVersion`。
