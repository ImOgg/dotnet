

# .NET Web API 學習筆記

## 環境安裝

### 1. 安裝 .NET SDK
- 前往 [官方網站](https://dotnet.microsoft.com/download) 下載
- 自行選擇適合的 .NET 版本

## 快速開始

### 1. 安裝依賴
```bash
dotnet restore
```

### 2. 設定資料庫連線
編輯 `appsettings.json`：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=c_test;user=root;password=yourpassword"
  }
}
```

### 2. VSCode 套件

| 套件名稱 | 說明 |
|----------|------|
| .NET Install Tool | 安裝與管理 .NET |
| C# | C# 語言支援 |
| C# Dev Kit | C# 開發工具包 我覺得這個不好用 |
| C# Extensions| 作者是 JosKreativ 可以直接新增template，我覺得比 C# Dev kit好用|
| NuGet Gallery | NuGet 套件管理 |
| Material Icon Theme | 檔案圖示美化 |
| SQLite | SQLite 支援（alexcvzz，可下 SQL query） |
| SQLite Viewer | SQLite 資料庫檢視器（唯讀，點兩下 .db 檔開啟） |
|json to ts|[好用工具](https://transform.tools/json-to-typescript)|

---
### 3. 資料庫
- 依照專案需求安裝 MySQL、PostgreSQL、MSSQL、Oracle 

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


## 參考文件
- [.NET 官方網站](https://dotnet.microsoft.com/)
- [.NET 版本支援政策](https://dotnet.microsoft.com/platform/support/policy)
- [從 .NET Framework 遷移指南](https://docs.microsoft.com/dotnet/core/porting/)