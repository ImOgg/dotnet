# .NET Web API 學習筆記

## 環境安裝

### 1. 安裝 .NET SDK
- 前往 [官方網站](https://dotnet.microsoft.com/download) 下載
- 自行選擇適合的 .NET 版本

### 2. VSCode 套件

| 套件名稱 | 說明 |
|----------|------|
| .NET Install Tool | 安裝與管理 .NET |
| C# | C# 語言支援 |
| C# Dev Kit | C# 開發工具包 |
| NuGet Gallery | NuGet 套件管理 |
| Material Icon Theme | 檔案圖示美化 |
| SQLite | SQLite 支援 |
| SQLite Viewer | SQLite 資料庫檢視器 |

---

## 常用指令

### 建立專案

```bash
dotnet new sln                        # 建立 Solution
dotnet new webapi -controllers        # 建立 Web API 專案（含 Controllers）
dotnet new webapi -h                  # 查看 webapi 說明
dotnet new list                       # 列出所有可用範本
dotnet run                            # 執行專案
```

### HTTPS 開發憑證

```bash
dotnet dev-certs https --trust        # 信任本機開發憑證
```

---

## Entity Framework Core

### 安裝套件

在 NuGet Gallery 搜尋並安裝（記得取消勾選 prerelease）：

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Sqlite`

> 安裝完成後，可到 `API.csproj` 確認是否成功安裝。

### 安裝 Migration 工具

```bash
# 全域安裝 dotnet-ef 工具
dotnet tool install --global dotnet-ef

# 確認版本
dotnet ef --version

# 日後升級 EF Core 套件時，同步更新工具
dotnet tool update --global dotnet-ef
```

### Migration 指令

```bash
dotnet ef                                              # 查看所有可用指令
dotnet ef migrations -h                                # 查看 migrations 子指令說明
dotnet ef migrations add InitialCreate -o Data/Migrations  # 新增 Migration
```

---

## 學習心得

- dotnet 沒有像 Laravel 那樣集中式管理 Route 的地方，通常會寫在 Controller 上
- `WeatherForecastController` 的路由規則：把 class 名稱的 `Controller` 去掉，貼到 URL 上就能看到 API
  - 例如：`WeatherForecastController` → `/WeatherForecast`
