# 文檔目錄

這個資料夾包含所有 ASP.NET Core 開發的完整指南。

## 第一階段：CRUD 基礎設置（先做這裡）

1. [00-foundation/01-environment-setup.md](./00-foundation/01-environment-setup.md) - 開發環境、資料庫連線、NuGet、Migration、Model/Controller 基礎流程
2. [01-crud-core/04-aspnet-overview.md](./01-crud-core/04-aspnet-overview.md) - 先理解 Controller / Route / Program.cs
3. [01-crud-core/05-migrations-guide.md](./01-crud-core/05-migrations-guide.md) - Entity 變更後如何更新資料庫
4. [01-crud-core/06-routing-guide.md](./01-crud-core/06-routing-guide.md) - CRUD 路由獨立指南（建議必讀）

先完成上面 1~3，再開始做實際 CRUD。

### .NET SDK / NuGet 自我檢查

在專案根目錄確認：

```bash
dotnet --info
dotnet nuget --help
```

若上面可執行，代表你可以使用 NuGet（不需要另外安裝 Composer）。

---

## 第二階段：補核心觀念

1. [00-foundation/02-dotnet-framework-vs-core.md](./00-foundation/02-dotnet-framework-vs-core.md) - .NET Framework vs .NET Core
2. [00-foundation/03-web-development-technologies.md](./00-foundation/03-web-development-technologies.md) - Web 開發技術
3. [01-crud-core/04-aspnet-overview.md](./01-crud-core/04-aspnet-overview.md) - ASP.NET 概覽
4. [01-crud-core/05-migrations-guide.md](./01-crud-core/05-migrations-guide.md) - Migration 指南

## 第三階段：進階主題（CRUD 跑通後再看）

6. [02-advanced/06-dependency-injection.md](./02-advanced/06-dependency-injection.md) - 依賴注入完整指南
7. [02-advanced/07-service-layer-architecture.md](./02-advanced/07-service-layer-architecture.md) - 服務層架構指南
8. [02-advanced/08-file-storage.md](./02-advanced/08-file-storage.md) - 檔案儲存與管理指南
9. [02-advanced/09-testing.md](./02-advanced/09-testing.md) - 測試完整指南
10. [02-advanced/10-background-jobs.md](./02-advanced/10-background-jobs.md) - 背景工作完整指南
11. [02-advanced/11-notifications.md](./02-advanced/11-notifications.md) - 通知系統完整指南

## 範例程式碼

請參考 `../API/Examples/` 資料夾:
- [README.md](../API/Examples/README.md) - 範例導覽
- 01-BasicWebAPI.md - 基本 Web API
- 02-DependencyInjection.md - 依賴注入範例
- 03-FileUpload.md - 檔案上傳範例
- 04-BackgroundJobs.md - 背景工作範例

## 快速查找

### 問題 → 文檔對照

| 問題 | 參考文檔 |
|------|---------|
| Import Job 怎麼做? | 02-advanced/10-background-jobs.md |
| Notifications 怎麼做? | 02-advanced/11-notifications.md |
| 依賴注入怎麼做? | 02-advanced/06-dependency-injection.md |
| 怎麼將方法抽出來整理? | 02-advanced/07-service-layer-architecture.md |
| Storage rules 可以分開寫嗎? | 02-advanced/08-file-storage.md |
| 測試怎麼寫? | 02-advanced/09-testing.md |

## 學習路徑

1. **第 1 步（必做）**: 00-foundation/01-environment-setup.md
2. **第 2 步（CRUD）**: 01-crud-core/04-aspnet-overview.md → 01-crud-core/06-routing-guide.md → 01-crud-core/05-migrations-guide.md → API/Examples/01-BasicWebAPI.md
3. **第 3 步（再進階）**: 02-advanced/06-dependency-injection.md → 02-advanced/07-service-layer-architecture.md
4. **第 4 步（後排）**: 02-advanced/09-testing.md → 02-advanced/10-background-jobs.md → 02-advanced/11-notifications.md
