# ASP.NET Web API 範例集

這個資料夾包含了完整的 ASP.NET Web API 開發範例。

## 目錄

1. **01-BasicWebAPI.md** - 基本 Web API 開發
   - 建立 Controller
   - CRUD 操作
   - HTTP 狀態碼
   - API 測試方法

2. **02-DependencyInjection.md** - 依賴注入完整範例
   - Repository 模式
   - Service 層
   - Controller 實作
   - 服務註冊

3. **03-FileUpload.md** - 檔案上傳
   - 單一檔案上傳
   - 多檔案上傳
   - 檔案驗證
   - 靜態檔案存取

4. **04-BackgroundJobs.md** - 背景工作
   - IHostedService
   - 背景任務佇列
   - 檔案匯入範例
   - 定期清理任務

## 快速開始

### 1. 建立專案
```bash
dotnet new webapi -n API
cd API
```

### 2. 安裝套件
```bash
# Entity Framework Core (MySQL)
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Design

# Swagger (已內建)
# dotnet add package Swashbuckle.AspNetCore
```

### 3. 設定資料庫連線
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydb;User=root;Password=yourpassword;"
  }
}
```

### 4. 執行專案
```bash
dotnet run
```

開啟瀏覽器: `https://localhost:5001/swagger`

## 專案結構

```
API/
├── Controllers/          # API 端點
├── Services/            # 業務邏輯
│   ├── Interfaces/
│   └── Implementations/
├── Repositories/        # 資料存取
│   ├── Interfaces/
│   └── Implementations/
├── Data/               # DbContext
├── Entities/           # 資料模型
├── DTOs/               # 資料傳輸物件
├── Examples/           # 範例文件 (這個資料夾)
└── Program.cs          # 應用程式進入點
```

## 相關文檔

完整的開發指南請參考 `docs/` 資料夾:

- `docs/10-background-jobs.md` - 背景工作完整指南
- `docs/06-dependency-injection.md` - 依賴注入完整指南
- `docs/07-service-layer-architecture.md` - 服務層架構指南
- `docs/08-file-storage.md` - 檔案儲存指南
- `docs/11-notifications.md` - 通知系統指南
- `docs/09-testing.md` - 測試指南

## ASP.NET 技術對照

### ASP.NET Web API (範例已提供)
- ✅ RESTful API 開發
- ✅ 依賴注入
- ✅ Entity Framework Core
- ✅ 背景工作

### ASP.NET Core MVC
- 與 Web API 類似,但加入 View (Razor)
- 適合傳統的伺服器端渲染網頁

### ASP.NET MVC (舊版 .NET Framework)
- 傳統 MVC 模式
- 需要 .NET Framework (非 .NET Core)
- 推薦改用 ASP.NET Core MVC

### ASP.NET WebForm (舊版)
- 事件驅動模型
- 已不推薦使用
- 推薦改用 ASP.NET Core

## 學習路徑

1. **基礎 Web API** (01-BasicWebAPI.md)
2. **依賴注入** (02-DependencyInjection.md)
3. **檔案上傳** (03-FileUpload.md)
4. **背景工作** (04-BackgroundJobs.md)
5. **進階主題** (參考 docs/ 資料夾)

## 常用指令

```bash
# 執行專案
dotnet run

# 執行並監聽檔案變更
dotnet watch run

# 建立 Migration
dotnet ef migrations add InitialCreate

# 更新資料庫
dotnet ef database update

# 執行測試
dotnet test

# 發佈專案
dotnet publish -c Release
```

## API 測試工具

1. **Swagger UI** (內建)
   - 開啟: https://localhost:5001/swagger

2. **Postman**
   - 下載: https://www.postman.com/

3. **VS Code REST Client**
   - 安裝擴充套件: REST Client
   - 建立 `.http` 檔案

4. **curl** (命令列)
   ```bash
   curl https://localhost:5001/api/users
   ```

## 疑難排解

### 無法連線資料庫
檢查 `appsettings.json` 的連線字串是否正確。

### CORS 錯誤
在 `Program.cs` 加入 CORS 設定:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

### 埠號已被使用
修改 `Properties/launchSettings.json` 的埠號設定。

## 更多資源

- [ASP.NET Core 官方文件](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core 文件](https://docs.microsoft.com/ef/core)
- [C# 語言參考](https://docs.microsoft.com/dotnet/csharp)
