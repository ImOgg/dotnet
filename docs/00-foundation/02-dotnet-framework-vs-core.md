# .NET Framework vs .NET Core 詳細比較

## 概述

### .NET Framework（傳統版本）
- **發布時間：** 2002年
- **平台支援：** 僅支援 Windows
- **最後版本：** .NET Framework 4.8（2019年，不再更新新功能）
- **狀態：** 進入維護模式，只修復安全性問題

### .NET Core（現代版本）
- **發布時間：** 2016年
- **平台支援：** 跨平台（Windows、Linux、macOS）
- **當前版本：** .NET 8.0（2023年）
- **狀態：** 持續更新，每年發布新版本

---

## 主要差異對照表

| 特性 | .NET Framework | .NET Core / .NET 5+ |
|------|---------------|-------------------|
| **平台支援** | 僅 Windows | Windows、Linux、macOS |
| **開源** | 部分開源 | 完全開源 |
| **效能** | 較慢 | 高效能（優化過） |
| **部署方式** | 需安裝 Framework | Self-contained 或 Framework-dependent |
| **體積** | 龐大（數百 MB） | 輕量、模組化 |
| **API 數量** | 約 ~400,000 | 約 ~200,000（但持續增加） |
| **更新頻率** | 已停止新功能 | 每年一個大版本 |
| **Container 支援** | 不友善 | 原生支援 Docker |
| **新專案建議** | ❌ 不建議 | ✅ 強烈建議 |

---

## 技術演進時間軸

```
2002 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 2019
     .NET Framework 1.0 → 4.8 (Windows Only)

2016 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 現在
     .NET Core 1.0 → 3.1 (Cross-platform)
                    ↓
     .NET 5 (2020) - 統一名稱，去掉 "Core"
                    ↓
     .NET 6 (2021) - LTS 版本
                    ↓
     .NET 7 (2022) - STS 版本
                    ↓
     .NET 8 (2023) - LTS 版本 ← 我們的專案
                    ↓
     .NET 9 (2024) - 計劃中
```

---

## 版本支援策略

### LTS (Long Term Support) - 長期支援版本
- **支援期限：** 3年
- **範例：** .NET 6, .NET 8
- **適合：** 生產環境、企業應用

### STS (Standard Term Support) - 標準支援版本
- **支援期限：** 18個月
- **範例：** .NET 7
- **適合：** 想嘗試新功能的專案

---

## 為什麼要從 .NET Framework 遷移到 .NET Core？

### 1. 效能提升
```csharp
// .NET Core 的效能是 .NET Framework 的 2-3 倍
// 特別是在 JSON 序列化、HTTP 請求等場景
```

### 2. 跨平台部署
```bash
# 可以在 Linux 伺服器上運行，節省授權費用
dotnet run  # Windows
dotnet run  # Linux
dotnet run  # macOS
```

### 3. 容器化支援
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . .
ENTRYPOINT ["dotnet", "API.dll"]
```

### 4. 現代化開發體驗
- 內建依賴注入（DI）
- 統一的配置系統
- 更好的日誌框架
- 原生支援異步程式設計

### 5. 持續更新
- 每年新增功能
- 安全性更新
- 社群活躍

---

## .NET Framework 仍然適用的場景

雖然 .NET Core/.NET 是未來趨勢，但以下情況可能仍需使用 .NET Framework：

1. **遺留系統維護**
   - 舊專案已經穩定運行
   - 遷移成本過高

2. **特定 Windows 技術**
   - WCF Server（.NET Core 只支援 Client）
   - WPF 桌面應用（.NET Core 3.0+ 才支援）
   - Windows Forms

3. **第三方套件相容性**
   - 某些老舊的 NuGet 套件只支援 .NET Framework

---

## 遷移建議

### 適合立即遷移：
- ✅ ASP.NET Web API
- ✅ ASP.NET MVC
- ✅ Console 應用程式
- ✅ Class Library

### 需要評估：
- ⚠️ WCF 服務（需改用 gRPC 或 REST）
- ⚠️ ASP.NET Web Forms（需重寫為 Razor Pages 或 Blazor）

### 不建議遷移：
- ❌ 非常老舊且穩定的系統
- ❌ 即將退役的應用程式

---

## 命名混淆說明

### 為什麼從 ".NET Core" 改名為 ".NET"？

Microsoft 在 .NET 5 開始統一命名：

```
舊命名方式：
- .NET Framework 4.8 (Windows)
- .NET Core 3.1 (Cross-platform)

新命名方式：
- .NET 5, 6, 7, 8... (統一為 "Cross-platform")
```

**原因：**
- 避免混淆
- 強調 .NET Core 是 .NET 的未來
- .NET Framework 進入維護模式

**注意：**
- 開發者仍習慣說 ".NET Core" 來區分
- 官方文件現在都用 ".NET"

---

## 總結

| 項目 | 建議 |
|------|------|
| **新專案** | 使用 .NET 8（或最新 LTS 版本）|
| **舊專案** | 評估遷移價值，優先遷移 Web API/MVC |
| **學習** | 專注於 .NET Core/.NET，.NET Framework 只需了解 |
| **部署** | 優先考慮 Linux + Docker 降低成本 |

---

## 參考資源

- [.NET 官方網站](https://dotnet.microsoft.com/)
- [.NET 版本支援政策](https://dotnet.microsoft.com/platform/support/policy)
- [從 .NET Framework 遷移指南](https://docs.microsoft.com/dotnet/core/porting/)
