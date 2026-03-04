# 環境設定與 Migration 編號流程

這份筆記是給目前專案使用（ASP.NET Core Web API + MySQL + Docker Compose）。

## 這份文件的目標（CRUD 起手式）

這份 `01` 要先完成以下基礎設置：

1. 開發環境可執行（Docker + SDK 容器 + NuGet）
2. DB 可連線，Migration 可套用
3. 有基本 Model（Entity）
4. 有基本 Controller（先能做 CRUD）

分層架構、測試、通知、背景工作，先放後面章節再做。

## 編號流程（照這個順序做）

1. 啟動服務

```bash
docker compose up -d --build
```

2. 確認 DB 健康

```bash
docker compose ps
```

3. 進 SDK 容器（不要進 `dotnet-api`）

```bash
docker run --rm -it --network dotnet_default -v "${PWD}:/src" -w /src/API mcr.microsoft.com/dotnet/sdk:8.0 sh
```

4. 先安裝 NuGet 套件（相當於 `composer install`）

```bash
dotnet restore
```

5. 第一次先裝 EF 工具

```bash
dotnet tool install --global dotnet-ef --version 8.*
export PATH=$PATH:/root/.dotnet/tools
```

6. 有改 Entity 才新增 Migration

```bash
dotnet ef migrations add <Name> -o Migrations
```

7. 套用到資料庫

```bash
dotnet ef database update
```

8. 離開容器

```bash
exit
```

9. 驗證資料表

```bash
docker exec dotnet-mysql mysql -uroot -ppassword -e "USE c_test; SHOW TABLES;"
```

10. 再開始寫 Controller

先做完第 9 步，再進入 API 功能開發。

11. 完成最小 CRUD 驗證（GET/POST 至少跑通）

確認 `Model ↔ DbContext ↔ Migration ↔ Controller` 這條線是通的。

---

## 30 秒口訣（先記這段就好）

1. `docker compose up -d --build`
2. 進 SDK 容器
3. `dotnet ef migrations add <Name> -o Migrations`
4. `dotnet ef database update`

你可以把它理解成：

- Laravel：`php artisan migrate`
- 本專案：`dotnet ef database update`（但在 SDK 容器裡執行）

---

## 我要進哪裡下指令？（重點）

- 要下 `dotnet add package`、`dotnet ef`、`dotnet restore`：**進 SDK 容器**。
- 不要在 `dotnet-api` 容器裡做開發指令（那是 runtime，主要拿來跑服務）。

進 SDK 容器指令（在專案根目錄執行）：

```bash
docker run --rm -it --network dotnet_default -v "${PWD}:/src" -w /src/API mcr.microsoft.com/dotnet/sdk:8.0 sh
```

進去後常用：

```bash
dotnet add package <PackageName>
dotnet restore
```

---

## 前置條件

- 先確認 `.NET SDK / NuGet` 可用（本機或 SDK 容器都可）：

```bash
dotnet --info
dotnet nuget --help
```

- 若你本機沒有 dotnet，請直接進 SDK 容器後再執行所有 `dotnet` 指令。

- 先啟動服務：

```bash
docker compose up -d --build
```

- 確認 MySQL 健康：

```bash
docker compose ps
```

看到 `dotnet-mysql` 狀態是 `healthy` 再跑 migration。

---

## 進容器後打短指令（開發推薦）

### Step 1) 進 SDK 容器

```bash
docker run --rm -it --network dotnet_default -v "${PWD}:/src" -w /src/API mcr.microsoft.com/dotnet/sdk:8.0 sh
```

### Step 2) 容器內第一次執行（每次新開容器都要）

```bash
dotnet tool install --global dotnet-ef --version 8.*
export PATH=$PATH:/root/.dotnet/tools
dotnet restore
```

### Step 3) 之後只要記這三個

```bash
dotnet ef migrations add <Name> -o Migrations
dotnet ef database update
dotnet ef migrations list
```

### Step 4) 離開容器

```bash
exit
```

---

## 常用指令對照（Laravel 思維）

| Laravel | .NET / EF Core |
|---|---|
| `php artisan make:migration` | `dotnet ef migrations add <Name>` |
| `php artisan migrate` | `dotnet ef database update` |
| `php artisan migrate:status` | `dotnet ef migrations list` |
| `php artisan migrate:rollback` | `dotnet ef database update <PreviousMigration>` |

---

## 驗證是否成功

### Workbench 檢查

連到：
- Host: `127.0.0.1`
- Port: `3307`
- DB: `c_test`

應可看到：
- `Users`
- `__EFMigrationsHistory`

### CLI 檢查

```bash
docker exec dotnet-mysql mysql -uroot -ppassword -e "USE c_test; SHOW TABLES; SELECT * FROM __EFMigrationsHistory;"
```

---

## 注意事項

- 本專案 DB 在 Docker 網路內主機名稱是 `mysql`，不是 `localhost`。
- 本機對外連線用 `3307`，容器內仍是 `3306`。
- `.env` 放在專案根目錄（和 `docker-compose.yml` 同層）是正確做法。
- `dotnet-api` 容器是 runtime image，通常不適合跑 `dotnet ef`；請用 SDK 容器。
