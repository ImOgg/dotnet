# .NET Web API 專案筆記

### 常用指令
```bash
# 建立解決方案
dotnet new sln

# 建立 Web API 專案（使用 Controller 模式）
dotnet new webapi -controllers -n API

# 將專案加入解決方案
dotnet sln add API

# 建立專案(一個c#的專案，非API)
    dotnet new console -n MyApp
#  修改 Entity 類別
dotnet ef migrations add AddUserAge

dotnet ef database update
```

要看更多可以去看 [01-environment-setup2.md](docs/00-foundation/01-environment-setup2.md)
---
## 重要檔案說明

### Program.cs
- ASP.NET Core 應用程式的入口點
- 用於註冊服務（Services）和中介軟體（Middleware）
- 應用程式的核心設定檔

### API.csproj
- 專案設定檔
- 透過 NuGet 安裝套件後，會自動在此檔案中新增相依項目

---


