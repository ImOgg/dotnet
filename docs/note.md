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
| SQLite | SQLite 支援（alexcvzz，可下 SQL query） |
| SQLite Viewer | SQLite 資料庫檢視器（唯讀，點兩下 .db 檔開啟） |

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
dotnet ef database update #執行Migration
```

---

## SQLite 查詢（alexcvzz 套件）

> SQLite Viewer 只能看資料（唯讀）；要下 query 用 SQLite (alexcvzz)。

1. `Ctrl+Shift+P` → `SQLite: Open Database` → 選 `.db` 檔
2. 左側 Explorer 最底部出現 **SQLITE EXPLORER**
3. 展開 table → 右鍵 → **New Query (Select)**
4. 會開啟一個 `--SQLite untitled-1` 未儲存檔案
5. 選取要執行的 SQL → 右鍵 **Run Selected Query**（或 `Ctrl+Shift+Q` 全部執行）

---

## Laravel vs .NET 對照

| Laravel | .NET | 說明 |
|---------|------|------|
| Model（`User.php`） | Entity（`AppUser.cs`） | 對應資料表的類別 |
| `$table = 'users'` | `DbSet<AppUser> Users` | 指定操作哪張資料表 |
| Eloquent ORM | Entity Framework Core | ORM 框架 |
| `User::all()` | `context.Users.ToList()` | 取全部資料 |
| `User::find($id)` | `context.Users.Find(id)` | 依 id 取單筆 |

### .NET 的差異

Laravel 的 Model 直接包含資料庫操作邏輯，.NET 拆成兩層：

- **Entity**（`Entities/AppUser.cs`）→ 單純定義資料結構，對應資料表欄位
- **DbContext**（`Data/AppDbContext.cs`）→ 負責資料庫操作，類似 Laravel 的 `DB` facade

---

## 學習心得

- dotnet 沒有像 Laravel 那樣集中式管理 Route 的地方，通常會寫在 Controller 上
- `WeatherForecastController` 的路由規則：把 class 名稱的 `Controller` 去掉，貼到 URL 上就能看到 API
  - 例如：`WeatherForecastController` → `/WeatherForecast`
