# 文檔目錄

這個資料夾包含所有 ASP.NET Core 開發的完整指南。

## 基礎概念

1. [dotnet-framework-vs-core.md](./dotnet-framework-vs-core.md) - .NET Framework vs .NET Core
2. [web-development-technologies.md](./web-development-technologies.md) - Web 開發技術
3. [aspnet-overview.md](./aspnet-overview.md) - ASP.NET 概覽
4. [migrations-guide.md](./migrations-guide.md) - Migration 指南
5. [docker-ef-migrations.md](./docker-ef-migrations.md) - Docker + EF Migration 實作筆記

## 進階主題

6. [background-jobs.md](./background-jobs.md) - 背景工作完整指南
7. [notifications.md](./notifications.md) - 通知系統完整指南
8. [dependency-injection.md](./dependency-injection.md) - 依賴注入完整指南
9. [service-layer-architecture.md](./service-layer-architecture.md) - 服務層架構指南
10. [file-storage.md](./file-storage.md) - 檔案儲存與管理指南
11. [testing.md](./testing.md) - 測試完整指南

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
| Import Job 怎麼做? | background-jobs.md |
| Notifications 怎麼做? | notifications.md |
| 依賴注入怎麼做? | dependency-injection.md |
| 怎麼將方法抽出來整理? | service-layer-architecture.md |
| Storage rules 可以分開寫嗎? | file-storage.md |
| 測試怎麼寫? | testing.md |

## 學習路徑

1. **新手**: aspnet-overview.md → dependency-injection.md
2. **進階**: service-layer-architecture.md → background-jobs.md
3. **實戰**: 參考 API/Examples/ 的範例程式碼
