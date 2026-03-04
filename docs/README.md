# 文檔目錄

這個資料夾包含所有 ASP.NET Core 開發的完整指南。

## 先做：環境設定（沒做完先不要往下）

1. [01-environment-setup.md](./01-environment-setup.md) - Docker、SDK 容器、NuGet 檢查、EF Migration、Workbench 連線
2. [.NET SDK / NuGet 自我檢查](#net-sdk--nuget-自我檢查)

先從 `01-environment-setup.md` 的「編號流程（照這個順序做）」開始，做完再往下讀其他文件。

### .NET SDK / NuGet 自我檢查

在專案根目錄確認：

```bash
dotnet --info
dotnet nuget --help
```

若上面可執行，代表你可以使用 NuGet（不需要另外安裝 Composer）。

---

## 基礎概念（環境 OK 後再讀）

1. [02-dotnet-framework-vs-core.md](./02-dotnet-framework-vs-core.md) - .NET Framework vs .NET Core
2. [03-web-development-technologies.md](./03-web-development-technologies.md) - Web 開發技術
3. [04-aspnet-overview.md](./04-aspnet-overview.md) - ASP.NET 概覽
4. [05-migrations-guide.md](./05-migrations-guide.md) - Migration 指南

## 進階主題

6. [06-dependency-injection.md](./06-dependency-injection.md) - 依賴注入完整指南
7. [07-service-layer-architecture.md](./07-service-layer-architecture.md) - 服務層架構指南
8. [08-file-storage.md](./08-file-storage.md) - 檔案儲存與管理指南
9. [09-testing.md](./09-testing.md) - 測試完整指南
10. [10-background-jobs.md](./10-background-jobs.md) - 背景工作完整指南
11. [11-notifications.md](./11-notifications.md) - 通知系統完整指南

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
| Import Job 怎麼做? | 10-background-jobs.md |
| Notifications 怎麼做? | 11-notifications.md |
| 依賴注入怎麼做? | 06-dependency-injection.md |
| 怎麼將方法抽出來整理? | 07-service-layer-architecture.md |
| Storage rules 可以分開寫嗎? | 08-file-storage.md |
| 測試怎麼寫? | 09-testing.md |

## 學習路徑

1. **第 0 步（必做）**: 01-environment-setup.md → `.NET SDK / NuGet 自我檢查`
2. **新手**: 04-aspnet-overview.md → 06-dependency-injection.md
3. **進階**: 07-service-layer-architecture.md → 10-background-jobs.md
4. **實戰**: 參考 API/Examples/ 的範例程式碼
