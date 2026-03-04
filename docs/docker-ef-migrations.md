# Docker 環境下的 EF Core Migration 筆記

這份筆記是給目前專案使用（ASP.NET Core Web API + MySQL + Docker Compose）。

## 30 秒口訣（先記這段就好）

1. `docker compose up -d --build`
2. 進 SDK 容器
3. 在容器內只打：
	- `dotnet ef migrations add <Name> -o Migrations`
	- `dotnet ef database update`
	- `dotnet ef migrations list`

你可以把它理解成：

- Laravel：`php artisan migrate`
- 本專案：`dotnet ef database update`（但在 SDK 容器裡執行）

---

## 前置條件

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

## 方式 A：一次性單行（不用進容器）

以下命令都在專案根目錄執行。

### 套用 migration（相當於 `php artisan migrate`）

```bash
docker run --rm --network dotnet_default -v "${PWD}:/src" -w /src/API -e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=c_test;User ID=root;Password=password;" mcr.microsoft.com/dotnet/sdk:8.0 sh -lc "dotnet tool install --global dotnet-ef --version 8.*; dotnet restore; /root/.dotnet/tools/dotnet-ef database update"
```

### 新增 migration（相當於 `make:migration`）

```bash
docker run --rm --network dotnet_default -v "${PWD}:/src" -w /src/API mcr.microsoft.com/dotnet/sdk:8.0 sh -lc "dotnet tool install --global dotnet-ef --version 8.*; dotnet restore; /root/.dotnet/tools/dotnet-ef migrations add AddSomething -o Migrations"
```

### 查看 migration 清單（相當於 `migrate:status`）

```bash
docker run --rm --network dotnet_default -v "${PWD}:/src" -w /src/API mcr.microsoft.com/dotnet/sdk:8.0 sh -lc "dotnet tool install --global dotnet-ef --version 8.*; /root/.dotnet/tools/dotnet-ef migrations list"
```

---

## 方式 B：進容器後打短指令（最好記）

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

## 我該選哪種？

- 想一次貼上就完成：用「方式 A」。
- 想少記長指令、像 artisan 操作：用「方式 B」。

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
