# .NET Web API 專案筆記

## 專案初始化

### 建立專案結構
```bash
# 建立解決方案
dotnet new sln

# 建立 Web API 專案（使用 Controller 模式）
dotnet new webapi -controllers -n API

# 將專案加入解決方案
dotnet sln add API

# 建立專案(一個c#的專案，非API)
    dotnet new console -n MyApp
```
### 開發工具與擴充套件
- **C# Dev Kit** - VS Code 的 C# 開發套件
- **C#** - 基本 C# 語言支援
- **.NET Install Tool** - .NET 安裝工具
- **NuGet Gallery** - 套件管理

---

## 執行與開發

### 啟動專案
```bash
# 執行專案
dotnet run

# 還原相依套件
dotnet restore
```

### Docker 快速開始（推薦）

本專案可直接使用 Docker 啟動 API + MySQL。

```bash
# 1) 第一次先建立環境檔（若已存在可略過）
copy .env.example .env

# 2) 啟動服務
docker compose up -d --build

# 3) 套用 Migration（建立資料表）
docker run --rm --network dotnet_default -v "${PWD}:/src" -w /src/API -e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=c_test;User ID=root;Password=password;" mcr.microsoft.com/dotnet/sdk:8.0 sh -lc "dotnet tool install --global dotnet-ef --version 8.*; dotnet restore; /root/.dotnet/tools/dotnet-ef database update"

# 4) 檢查資料表
docker exec dotnet-mysql mysql -uroot -ppassword -e "USE c_test; SHOW TABLES;"
```

MySQL Workbench 連線資訊：
- Host: `127.0.0.1`
- Port: `3307`
- User: `root`
- Password: `.env` 的 `MYSQL_ROOT_PASSWORD`
- Database: `c_test`

更多 Docker + EF 操作，請看 `docs/docker-ef-migrations.md`。

### Launch Settings 設定
位置：`Properties/launchSettings.json`

可以刪除多餘的設定檔，只保留需要的一個。建議保留的 HTTP 設定：

```json
"http": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "http://localhost:5258",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

---

## ASP.NET Core 命名與路由慣例

### Controller 命名規則

1. **Controller 類別命名**
   - Controller 類別名稱必須以 `Controller` 結尾
   - 範例：`WeatherForecastController`、`UsersController`、`ProductsController`

2. **`[Route("[controller]")]` 的作用**
   - `[controller]` 是一個佔位符（token）
   - 框架會自動取 Controller 名稱，並去掉 `Controller` 後綴
   - 範例：
     - `WeatherForecastController` → 路由為 `/WeatherForecast`
     - `UsersController` → 路由為 `/Users`

3. **為什麼這樣設計？**
   - ✅ 減少重複代碼：不用每個 Controller 都手寫路由
   - ✅ 保持一致性：所有 Controller 遵循相同的命名規則
   - ✅ 易於維護：改 Controller 名稱，路由會自動跟著改

### 其他常見路由寫法

```csharp
// 加上 api 前綴（很常見的做法）
[Route("api/[controller]")]
// 路由結果：/api/WeatherForecast

// 手動指定完整路由（不使用慣例）
[Route("api/weather")]
// 路由結果：/api/weather

// 包含版本號
[Route("api/v1/[controller]")]
// 路由結果：/api/v1/WeatherForecast
```

---

## 重要檔案說明

### API.http
- HTTP request 測試檔案（`.http` 格式）
- 可在 VS Code 中直接測試 API endpoints
- 無需使用 Postman 等外部工具

### Program.cs
- ASP.NET Core 應用程式的入口點
- 用於註冊服務（Services）和中介軟體（Middleware）
- 應用程式的核心設定檔

### API.csproj
- 專案設定檔
- 透過 NuGet 安裝套件後，會自動在此檔案中新增相依項目

---

## 常見問題與解決方案

### 解決 HTTPS 憑證警告

如果開發過程中瀏覽器顯示「不安全」的警告：

```bash
# 清除現有的開發憑證
dotnet dev-certs https --clean

# 信任新的開發憑證
dotnet dev-certs https --trust

# 查看更多憑證相關指令
dotnet dev-certs https -h
```

執行完畢後，重新啟動專案即可。

---

## 套件管理

### 使用 NuGet 安裝套件

透過 NuGet 安裝的套件會自動更新到 `API.csproj` 檔案中。

```bash
# 搜尋套件（在 VS Code 的 NuGet Gallery）
# 或使用指令：
dotnet add package [套件名稱]
```

# 安裝 SQL Server 套件
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# 安裝 PostgreSQL 套件
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
---

## MySQL 資料庫設定

### 1. 安裝必要套件

已安裝以下 NuGet 套件來支援 MySQL:

```bash
# 安裝 Pomelo MySQL Provider (版本 9.0.0)
dotnet add package Pomelo.EntityFrameworkCore.MySql

# 安裝 EF Core Design 工具 (版本 9.0.10) - 用於資料庫遷移
dotnet add package Microsoft.EntityFrameworkCore.Design

# 安裝全域 EF Core CLI 工具 (版本 9.0.10)
dotnet tool install --global dotnet-ef
```

### 2. 建立 Data 資料夾與 DbContext

**位置:** `API/Data/ApplicationDbContext.cs`

```csharp
using System;
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

        // 定義你的資料表
        public DbSet<Product> Products { get; set; }
    }
}
```

### 3. 建立 Models 資料夾與實體類別

**位置:** `API/Models/Product.cs`

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

### 4. 設定連接字串

**位置:** `API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=MyDatabase;User=root;Password=your_password;"
  }
}
```

**重要:** 請將 `your_password` 替換成你的 MySQL root 密碼,或使用其他 MySQL 用戶。

### 5. 在 Program.cs 中註冊 DbContext

**位置:** `API/Program.cs`

在 `builder.Services.AddSwaggerGen();` 之後新增:

```csharp
// MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 21))
    )
);
```

**注意:** 使用 `MySqlServerVersion` 指定 MySQL 版本,而不是 `ServerVersion.AutoDetect()`,以避免在設計時連接資料庫的問題。

### 6. 建立資料庫遷移

```bash
# 建立初始遷移檔案
cd API
dotnet ef migrations add InitialCreate
```

執行成功後,會在 `API/Migrations` 資料夾中產生遷移檔案。

### 7. 更新資料庫

**在執行此步驟前,請確保:**
1. 已安裝並啟動 MySQL Server
2. 已更新 `appsettings.json` 中的連接字串 (特別是密碼)
3. MySQL 用戶具有建立資料庫的權限

```bash
# 執行遷移,建立資料庫與資料表
dotnet ef database update
```

### 8. 驗證連接

執行專案並檢查是否成功連接到 MySQL:

```bash
dotnet run
```

---

### 常見問題

#### Q: 遇到「Access denied for user」錯誤
**A:** 檢查 `appsettings.json` 中的連接字串,確認用戶名稱和密碼是否正確。

#### Q: 遇到「Unknown database」錯誤
**A:** 執行 `dotnet ef database update` 會自動建立資料庫。如果仍有問題,可以手動在 MySQL 中建立:
```sql
CREATE DATABASE MyDatabase;
```

#### Q: 如何查看 MySQL 版本?
**A:** 在 MySQL 命令列執行:
```sql
SELECT VERSION();
```
如果你的 MySQL 版本不是 8.0.21,請在 `Program.cs` 中更新為對應版本。

---

###  簡化寫using

```bash
# Global.cs 
using Global  w1.Models
```

```bash
# API.csproj
<ItemGroup>
<Using Include="w1.Models">
</ItemGroup>
```

## 參考資源

- [ASP.NET Core 官方文件](https://learn.microsoft.com/zh-tw/aspnet/core/)
- [.NET CLI 指令參考](https://learn.microsoft.com/zh-tw/dotnet/core/tools/)
