# ASP.NET 完整說明

## .NET Framework、.NET Core、ASP.NET 三者關係

### 最簡單的理解方式

```
🏗️ .NET Framework / .NET Core  =  整棟房子（執行平台）
              ↓
    🍳 ASP.NET / ASP.NET Core   =  廚房（Web 開發專用區域）
              ↓
  🔧 Web API / MVC / Blazor     =  廚具（具體的開發工具）
```

### 完整關係圖

```
┌─────────────────────────────────────────────────────────┐
│                    .NET 生態系統                          │
│                                                          │
│  ┌──────────────────┐         ┌──────────────────┐     │
│  │  .NET Framework  │         │    .NET Core     │     │
│  │   (2002-2019)    │         │   (2016-2020)    │     │
│  │                  │         │        ↓         │     │
│  │   Windows Only   │         │     .NET 5+      │     │
│  │                  │         │   (2020-現在)     │     │
│  └────────┬─────────┘         └────────┬─────────┘     │
│           │                            │                │
│           ↓                            ↓                │
│  ┌──────────────────┐         ┌──────────────────┐     │
│  │    ASP.NET       │         │  ASP.NET Core    │     │
│  │  (Web 框架)       │         │  (Web 框架)       │     │
│  │  - Web Forms     │         │  - MVC           │     │
│  │  - MVC           │         │  - Web API       │     │
│  │  - Web API       │         │  - Razor Pages   │     │
│  │                  │         │  - Blazor        │     │
│  └──────────────────┘         └──────────────────┘     │
└─────────────────────────────────────────────────────────┘
```

### 三者定義

| 名稱 | 定義 | 範圍 | 比喻 |
|------|------|------|------|
| **.NET Framework** | 執行平台 + 開發框架 | 整個生態系統 | 整棟房子 |
| **.NET Core/.NET** | 新一代執行平台 + 框架 | 整個生態系統 | 現代化房子 |
| **ASP.NET** | Web 開發框架 | .NET 中的 Web 部分 | 房子裡的廚房 |
| **ASP.NET Core** | 新一代 Web 框架 | .NET Core 中的 Web 部分 | 現代廚房 |

### 層級關係

```
第一層：平台（Platform）
┌─────────────────────────────────────────────────┐
│  .NET Framework (舊)      .NET Core/.NET (新)    │
│  - Windows Only          - 跨平台               │
│  - 2002-2019            - 2016-現在             │
└─────────────────────────────────────────────────┘
                          │
                          ↓
第二層：應用類型框架
┌─────────────────────────────────────────────────┐
│  ASP.NET / ASP.NET Core                         │
│  (專門做 Web 的框架)                              │
└─────────────────────────────────────────────────┘
                          │
                          ↓
第三層：具體技術
┌─────────────────────────────────────────────────┐
│  Web Forms | MVC | Web API | Razor | Blazor    │
│  (不同的 Web 開發方式)                            │
└─────────────────────────────────────────────────┘
```

### 你的專案技術棧

```
你使用的技術：
┌─────────────────────────────────┐
│ Platform: .NET 8.0              │  ← 執行平台（房子）
└─────────────────────────────────┘
                ↓
┌─────────────────────────────────┐
│ Framework: ASP.NET Core         │  ← Web 框架（廚房）
└─────────────────────────────────┘
                ↓
┌─────────────────────────────────┐
│ Type: Web API                   │  ← 具體技術（烤箱）
└─────────────────────────────────┘

完整名稱：ASP.NET Core Web API on .NET 8.0
```

### 快速對照表

| 項目 | .NET Framework | .NET Core/.NET | ASP.NET | ASP.NET Core |
|------|---------------|----------------|---------|--------------|
| **類型** | 執行平台 | 執行平台 | Web 框架 | Web 框架 |
| **範圍** | 整個生態系統 | 整個生態系統 | Web 開發部分 | Web 開發部分 |
| **平台支援** | Windows Only | 跨平台 | Windows Only | 跨平台 |
| **關係** | 包含 ASP.NET | 包含 ASP.NET Core | 基於 .NET Fwk | 基於 .NET Core |
| **狀態** | 停止更新 | 持續更新 | 停止更新 | 持續更新 |

---

## 什麼是 ASP.NET？

**ASP.NET** 是 Microsoft 開發的一個 **Web 應用程式開發框架**，用於建立動態網站、Web 應用程式和 Web 服務。

**重要：** ASP.NET 不是一個獨立的平台，而是 .NET Framework/.NET Core 中專門用來做 Web 開發的部分。

---

## ASP.NET 的演進歷史

### 完整時間軸

```
1996 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 2002
     ASP (Active Server Pages)
     - 使用 VBScript/JScript
     - 沒有物件導向
     - 混亂的程式碼結構

2002 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 2009
     ASP.NET Web Forms (.NET Framework)
     - 第一個真正的 ASP.NET
     - 事件驅動模型
     - ViewState 機制

2009 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 2012
     ASP.NET MVC
     - 引入 MVC 設計模式
     - 更好的測試性
     - RESTful 路由

2012 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 2016
     ASP.NET Web API
     - 專注於 API 開發
     - 返回 JSON/XML
     - 輕量化

2016 ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 現在
     ASP.NET Core
     - 跨平台
     - 開源
     - 高效能
     - 統一 MVC 和 Web API
     - 加入 Razor Pages (2017)
     - 加入 Blazor (2018)
```

---

## ASP.NET 家族成員

### 1. ASP (經典 ASP) - 已淘汰
**年代：** 1996-2002

```asp
<!-- 經典 ASP 程式碼（VBScript）-->
<%
Dim name
name = Request.Form("username")
Response.Write "Hello, " & name
%>
```

**特點：**
- ❌ 沒有物件導向
- ❌ 程式碼與 HTML 混在一起
- ❌ 難以維護
- ❌ 已經完全過時

---

### 2. ASP.NET (完整名稱)

**ASP.NET** 是一個總稱，包含以下所有技術：

#### 2.1 ASP.NET Web Forms
```csharp
// .aspx.cs (Code-behind)
protected void Button1_Click(object sender, EventArgs e)
{
    Label1.Text = "Hello, " + TextBox1.Text;
}
```

#### 2.2 ASP.NET MVC
```csharp
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

#### 2.3 ASP.NET Web API
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(users);
    }
}
```

#### 2.4 ASP.NET Core
- 上述所有技術的現代版本
- 跨平台、高效能
- 統一了 MVC 和 Web API

---

## ASP.NET vs ASP.NET Core

### 架構圖

```
┌────────────────────────────────────────────────────┐
│                   ASP.NET 家族                      │
└────────────────────────────────────────────────────┘
                        │
        ┌───────────────┴───────────────┐
        │                               │
┌───────▼────────┐            ┌─────────▼─────────┐
│  ASP.NET       │            │  ASP.NET Core     │
│  (.NET Fwk)    │            │  (.NET Core/.NET) │
└────────────────┘            └───────────────────┘
        │                               │
        ├─ Web Forms                    ├─ MVC
        ├─ MVC                          ├─ Web API
        ├─ Web API                      ├─ Razor Pages
        └─ Web Pages                    ├─ Blazor
                                        └─ gRPC
```

### 對照表

| 特性 | ASP.NET | ASP.NET Core |
|------|---------|--------------|
| **平台** | Windows Only | 跨平台 |
| **運行環境** | .NET Framework | .NET Core/.NET |
| **開源** | 部分 | 完全 |
| **效能** | 一般 | 非常快 |
| **依賴注入** | 需額外套件 | 內建 |
| **模組化** | 否 | 是 |
| **容器支援** | 不友善 | 完整支援 |
| **雲端部署** | 可以但不理想 | 優化過 |
| **新功能** | 停止更新 | 持續更新 |

---

## ASP.NET Core 的優勢

### 1. 跨平台

**傳統 ASP.NET：**
```
只能在 Windows + IIS 上運行
```

**ASP.NET Core：**
```
✅ Windows (IIS, Kestrel)
✅ Linux (Nginx, Apache)
✅ macOS (開發環境)
✅ Docker 容器
✅ Kubernetes
```

### 2. 高效能

ASP.NET Core 是最快的 Web 框架之一：

```
TechEmpower Benchmark 測試結果 (每秒請求數)：

ASP.NET Core:  7,000,000+ req/sec
Node.js:         600,000 req/sec
Django:          150,000 req/sec
Rails:            50,000 req/sec
```

### 3. 統一的程式模型

**傳統 ASP.NET（分離）：**
```csharp
// MVC Controller
public class HomeController : Controller { }

// Web API Controller
public class ApiController : ApiController { }  // 不同的基底類別！
```

**ASP.NET Core（統一）：**
```csharp
// MVC Controller
public class HomeController : Controller { }

// Web API Controller
public class ApiController : ControllerBase { }  // 統一的架構！
```

### 4. 內建依賴注入（DI）

**傳統 ASP.NET：**
```csharp
// 需要安裝第三方套件（Unity, Autofac, Ninject）
```

**ASP.NET Core：**
```csharp
// Program.cs - 內建 DI
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<ApplicationDbContext>();

// Controller 自動注入
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;  // 自動注入！
    }
}
```

### 5. 模組化設計

**傳統 ASP.NET：**
```
安裝整個 .NET Framework (幾百 MB)
```

**ASP.NET Core：**
```csharp
// 只安裝需要的 NuGet 套件
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Swashbuckle.AspNetCore" />

// 最小化的應用程式可以只有幾 MB
```

### 6. 中介軟體管道 (Middleware Pipeline)

**ASP.NET Core 的請求處理流程：**

```csharp
// Program.cs
var app = builder.Build();

// 請求從上到下通過每個 Middleware
app.UseHttpsRedirection();      // ① HTTPS 重導向
app.UseStaticFiles();            // ② 靜態檔案處理
app.UseRouting();                // ③ 路由匹配
app.UseAuthentication();         // ④ 身份驗證
app.UseAuthorization();          // ⑤ 授權檢查
app.MapControllers();            // ⑥ 執行 Controller

app.Run();
```

**請求流程圖：**
```
HTTP Request
     ↓
[HTTPS Redirect] ──→ 如果是 HTTP，重導向到 HTTPS
     ↓
[Static Files]   ──→ 如果是靜態檔案 (.css, .js)，直接返回
     ↓
[Routing]        ──→ 匹配路由 (api/users/123)
     ↓
[Authentication] ──→ 驗證身份 (JWT Token)
     ↓
[Authorization]  ──→ 檢查權限 (是否有權限)
     ↓
[Controller]     ──→ 執行 Action
     ↓
HTTP Response
```

---

## ASP.NET Core 專案結構

### 典型的 Web API 專案（你的專案）

```
API/
├── Controllers/           # API 控制器
│   └── UsersController.cs
├── Data/                 # 資料庫相關
│   └── ApplicationDbContext.cs
├── Entities/             # 資料模型
│   └── AppUser.cs
├── Migrations/           # EF Core Migrations
│   └── 20251020090313_InitialCreateAppUser.cs
├── docs/                 # 文件
│   ├── 02-dotnet-framework-vs-core.md
│   ├── 03-web-development-technologies.md
│   └── 04-aspnet-overview.md
├── appsettings.json      # 配置檔
├── Program.cs            # 應用程式入口
└── API.csproj            # 專案檔
```

### Program.cs 解析（你的專案核心）

```csharp
// ============ 建立 Builder ============
var builder = WebApplication.CreateBuilder(args);

// ============ 註冊服務 (DI Container) ============
// 加入 Controller 支援
builder.Services.AddControllers();

// 加入 Swagger (API 文件)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 加入 Entity Framework Core + MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(connectionString)
    )
);

// ============ 建立應用程式 ============
var app = builder.Build();

// ============ 配置 Middleware Pipeline ============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // 啟用 Swagger
    app.UseSwaggerUI();    // Swagger UI
}

app.UseHttpsRedirection();  // HTTPS 重導向
app.UseAuthorization();     // 授權
app.MapControllers();       // 映射 Controller 路由

// ============ 啟動應用程式 ============
app.Run();
```

---

## ASP.NET Core 的關鍵概念

### 1. 依賴注入 (Dependency Injection)

**生命週期：**

```csharp
// Transient - 每次請求都建立新實例
builder.Services.AddTransient<IEmailService, EmailService>();

// Scoped - 每個 HTTP 請求建立一個實例（最常用）
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDbContext<ApplicationDbContext>();  // Scoped

// Singleton - 應用程式啟動時建立一次，全域共用
builder.Services.AddSingleton<IConfiguration>(configuration);
```

**使用範例：**

```csharp
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    // Constructor Injection - 自動注入
    public UsersController(
        ApplicationDbContext context,
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        _logger.LogInformation("Getting all users");
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }
}
```

### 2. 配置系統 (Configuration)

**appsettings.json：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=c_test;user=root;password=xxx"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AppSettings": {
    "JwtSecret": "your-secret-key",
    "TokenExpirationDays": 7
  }
}
```

**讀取配置：**
```csharp
// 直接讀取
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSecret = builder.Configuration["AppSettings:JwtSecret"];

// 綁定到類別 (推薦)
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings")
);

// 在 Controller 中使用
public class AuthController : ControllerBase
{
    private readonly AppSettings _appSettings;

    public AuthController(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }
}
```

### 3. 路由系統

**屬性路由 (Attribute Routing)：**

```csharp
[ApiController]
[Route("api/[controller]")]  // api/users
public class UsersController : ControllerBase
{
    // GET: api/users
    [HttpGet]
    public IActionResult GetAll() { }

    // GET: api/users/123
    [HttpGet("{id}")]
    public IActionResult GetById(string id) { }

    // GET: api/users/search?name=john
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string name) { }

    // POST: api/users
    [HttpPost]
    public IActionResult Create([FromBody] AppUser user) { }

    // PUT: api/users/123
    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] AppUser user) { }

    // DELETE: api/users/123
    [HttpDelete("{id}")]
    public IActionResult Delete(string id) { }
}
```

**路由模板說明：**

| 模板 | URL | 說明 |
|------|-----|------|
| `[controller]` | users | Controller 名稱（去掉 Controller 後綴）|
| `{id}` | 123 | 路由參數 |
| `{id:int}` | 123 | 限制為整數 |
| `{id:guid}` | abc-123 | 限制為 GUID |
| `search/{keyword?}` | search 或 search/john | 可選參數 |

### 4. 模型綁定與驗證

```csharp
using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required(ErrorMessage = "名稱為必填")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "名稱長度需在 2-100 之間")]
    public string DisplayName { get; set; }

    [Required(ErrorMessage = "Email 為必填")]
    [EmailAddress(ErrorMessage = "Email 格式不正確")]
    public string Email { get; set; }

    [Range(18, 120, ErrorMessage = "年齡需在 18-120 之間")]
    public int Age { get; set; }
}

[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    // 自動驗證
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);  // 返回驗證錯誤
    }

    // 處理邏輯...
    return Ok();
}
```

### 5. 錯誤處理

**全域錯誤處理：**

```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // 開發環境：詳細錯誤
}
else
{
    app.UseExceptionHandler("/error");  // 生產環境：隱藏錯誤細節
}

// ErrorController.cs
[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleError()
    {
        return Problem();  // 返回標準 RFC 7807 錯誤格式
    }
}
```

**自訂 Middleware 處理錯誤：**

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            error = "Internal Server Error",
            message = exception.Message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}

// 註冊 Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
```

---

## ASP.NET Core vs 其他框架

### 效能比較

| 框架 | 語言 | 每秒請求數 | 相對速度 |
|------|------|-----------|---------|
| **ASP.NET Core** | C# | 7,000,000+ | 🔥🔥🔥🔥🔥 |
| Go (net/http) | Go | 6,000,000 | 🔥🔥🔥🔥🔥 |
| Fastify | Node.js | 600,000 | 🔥🔥 |
| Express.js | Node.js | 400,000 | 🔥 |
| Spring Boot | Java | 300,000 | 🔥 |
| Django | Python | 150,000 | 🔥 |
| Ruby on Rails | Ruby | 50,000 | 💤 |

### 生態系比較

| 特性 | ASP.NET Core | Node.js | Django | Spring Boot |
|------|--------------|---------|--------|-------------|
| **學習曲線** | 中等 | 簡單 | 簡單 | 陡峭 |
| **效能** | 極高 | 中等 | 低 | 中高 |
| **型別安全** | ✅ 強型別 | ❌ 弱型別 | ✅ (Type hints) | ✅ 強型別 |
| **非同步** | ✅ async/await | ✅ Promise | ✅ async/await | ✅ Reactive |
| **ORM** | EF Core | Sequelize | Django ORM | JPA/Hibernate |
| **生態系** | 成熟 | 非常豐富 | 豐富 | 非常成熟 |
| **就業市場** | 企業主流 | 新創主流 | 中等 | 企業主流 |

---

## 總結

### ASP.NET 是什麼？
**ASP.NET** 是 Microsoft 的 **Web 開發框架家族**，包含：
- Web Forms（舊）
- MVC
- Web API
- **ASP.NET Core**（現代、推薦）

### ASP.NET Core 的核心優勢
1. ✅ **跨平台**：Windows、Linux、macOS
2. ✅ **高效能**：世界最快的 Web 框架之一
3. ✅ **開源**：完全開源，社群活躍
4. ✅ **現代化**：內建 DI、統一架構、模組化
5. ✅ **雲端友善**：Docker、Kubernetes、微服務

### 你的專案
- **技術棧：** ASP.NET Core 8.0 Web API
- **優勢：** 最現代、最推薦的後端開發方式
- **適合：** 前後端分離、微服務、RESTful API

---

## 參考資源

- [ASP.NET Core 官方文件](https://docs.microsoft.com/aspnet/core)
- [ASP.NET Core GitHub](https://github.com/dotnet/aspnetcore)
- [ASP.NET Core 效能測試](https://www.techempower.com/benchmarks/)
