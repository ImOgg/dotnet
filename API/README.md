# API 專案

ASP.NET Core Web API 專案，使用 .NET 8.0 + Entity Framework Core + MySQL。

---

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

### 3. 執行 Migration
```bash
dotnet ef database update
```

### 4. 啟動專案
```bash
dotnet run
```

專案將會在 `https://localhost:5001` 啟動。

---

## 專案資訊

| 項目 | 說明 |
|------|------|
| **.NET 版本** | .NET 8.0 |
| **框架** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 9.0.10 |
| **資料庫** | MySQL |
| **資料庫提供者** | Pomelo.EntityFrameworkCore.MySql 9.0.0 |

---

## 專案結構

```
API/
├── Controllers/          # API 控制器
│   └── UsersController.cs
├── Data/                # 資料庫 Context
│   └── ApplicationDbContext.cs
├── Entities/            # 資料模型
│   └── AppUser.cs
├── Migrations/          # EF Core Migrations
├── docs/                # 技術文件（位於根目錄）
├── appsettings.json     # 配置檔
└── Program.cs           # 應用程式入口
```

---

## 📚 技術文件

詳細的技術文件存放在 **[../docs/](../docs/)** 資料夾：

### 核心概念
- **[ASP.NET 完整說明](../docs/01-crud-core/04-aspnet-overview.md)**
  - .NET Framework、.NET Core、ASP.NET 三者關係
  - ASP.NET 演進歷史
  - ASP.NET vs ASP.NET Core 詳細比較
  - 核心概念：DI、配置、路由、中介軟體
  - Program.cs 完整解析

- **[.NET Framework vs .NET Core](../docs/00-foundation/02-dotnet-framework-vs-core.md)**
  - 版本演進時間軸
  - 主要差異對照表
  - LTS vs STS 支援策略
  - 遷移建議

### Web 開發技術
- **[ASP.NET Web 開發技術完整指南](../docs/00-foundation/03-web-development-technologies.md)**
  - Web Forms、MVC、Web API、Razor Pages、Blazor
  - 各技術適用場景與程式碼範例
  - 技術選擇決策樹

- **[CRUD 路由指南](../docs/01-crud-core/06-routing-guide.md)**
  - `[Route]`、`[HttpGet]`、`[HttpPost]` 等最小路由實作
  - CRUD 路徑與狀態碼對照
  - 常見 404/400 問題排查

### 資料庫
- **[Entity Framework Core Migration 指南](../docs/01-crud-core/05-migrations-guide.md)**
  - Migration 基本概念與命令
  - 完整工作流程與專案紀錄
  - 進階技巧與最佳實踐
  - 常見問題解決方案

---

## 路由方式（先記這個）

ASP.NET Core 常見有兩種路由寫法：

1. **Controller 路由（本專案目前使用）**
  - 在 Controller 上用 `[Route]`、`[HttpGet]`、`[HttpPost]` 等屬性
  - 優點：結構清楚，CRUD 與團隊協作常用

2. **Minimal API 路由**
  - 在 `Program.cs` 內用 `app.MapGet()`、`app.MapPost()` 集中定義
  - 優點：檔案少、快速開發小型 API

Laravel 對照：
- `routes/api.php`（集中式）比較接近 Minimal API
- Controller 屬性路由是 ASP.NET Core Web API 最常見做法

---

## 常用命令

### Entity Framework Core

```bash
# 創建 Migration
dotnet ef migrations add <MigrationName>

# 套用 Migration
dotnet ef database update

# 查看 Migrations
dotnet ef migrations list

# 移除最後一個 Migration（未套用）
dotnet ef migrations remove

# 回滾到特定 Migration
dotnet ef database update <MigrationName>
```

### 開發

```bash
# 啟動專案
dotnet run

# 監聽模式（自動重啟）
dotnet watch run

# 建置專案
dotnet build

# 發布專案
dotnet publish -c Release
```

---

## API 端點

### Users API

| 方法 | 路由 | 說明 |
|------|------|------|
| GET | `/api/users` | 取得所有使用者 |
| GET | `/api/users/{id}` | 取得特定使用者 |
| POST | `/api/users` | 建立新使用者 |
| PUT | `/api/users/{id}` | 更新使用者 |
| DELETE | `/api/users/{id}` | 刪除使用者 |

---

## 開發環境

### 必要工具
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [MySQL](https://www.mysql.com/)
- [Visual Studio Code](https://code.visualstudio.com/) 或 [Visual Studio 2022](https://visualstudio.microsoft.com/)

### 推薦 VS Code 擴充
- C# Dev Kit
- REST Client
- GitLens

---

## 授權

此專案僅供學習使用。
