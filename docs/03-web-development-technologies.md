# ASP.NET Web 開發技術完整指南

## 技術概覽

ASP.NET 提供多種 Web 開發技術，適用於不同的應用場景。

---

## 1. ASP.NET Web Forms

### 基本資訊
- **推出年份：** 2002
- **平台：** .NET Framework
- **狀態：** ❌ 已過時，不建議新專案使用
- **最後更新：** .NET Framework 4.8

### 特色
- 🎯 模仿 Windows Forms 的開發體驗
- 拖拉式控制項（Drag & Drop）
- 事件驅動模型（Event-driven）
- 使用 ViewState 管理狀態
- Server Controls（伺服器控制項）

### 範例程式碼

**ASPX 頁面：**
```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" %>

<!DOCTYPE html>
<html>
<body>
    <form id="form1" runat="server">
        <asp:Label ID="lblMessage" runat="server" Text="Hello"></asp:Label>
        <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
        <asp:Button ID="btnSubmit" runat="server" Text="Submit"
                    OnClick="btnSubmit_Click" />
    </form>
</body>
</html>
```

**Code-Behind (Default.aspx.cs)：**
```csharp
public partial class Default : System.Web.UI.Page
{
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        lblMessage.Text = "Hello, " + txtName.Text;
    }
}
```

### 優點
- ✅ 對 Windows Forms 開發者友善
- ✅ 快速開發簡單頁面
- ✅ 豐富的內建控制項

### 缺點
- ❌ ViewState 導致頁面肥大
- ❌ HTML 控制權低
- ❌ 不適合 RESTful API
- ❌ 測試困難
- ❌ 不支援現代前端框架（React、Vue）
- ❌ 只能在 .NET Framework 上運行

### 適用場景
- 僅限維護舊系統
- 不建議任何新專案使用

---

## 2. ASP.NET MVC (Model-View-Controller)

### 基本資訊
- **推出年份：** 2009
- **平台：** .NET Framework → .NET Core/.NET
- **狀態：** ✅ 仍然廣泛使用
- **當前版本：** ASP.NET Core MVC（整合在 .NET 中）

### 特色
- 🎯 採用 MVC 設計模式
- Server-side rendering
- Razor 視圖引擎（.cshtml）
- RESTful 路由
- 完整的 HTML 控制權
- 可測試性高

### 架構說明

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ HTTP Request
       ↓
┌─────────────────────────────┐
│      Controller             │
│  - 處理請求                  │
│  - 呼叫 Model               │
│  - 選擇 View                │
└──────┬──────────────────────┘
       │
       ↓
┌─────────────┐      ┌─────────────┐
│   Model     │←────→│    View     │
│  - 資料邏輯  │      │  - UI 顯示   │
│  - 業務邏輯  │      │  - Razor     │
└─────────────┘      └─────────────┘
```

### 範例程式碼

**Controller (HomeController.cs)：**
```csharp
public class HomeController : Controller
{
    private readonly IUserService _userService;

    public HomeController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: /Home/Index
    public IActionResult Index()
    {
        var users = _userService.GetAllUsers();
        return View(users);
    }

    // GET: /Home/Details/5
    public IActionResult Details(int id)
    {
        var user = _userService.GetUserById(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    // POST: /Home/Create
    [HttpPost]
    public IActionResult Create(User user)
    {
        if (ModelState.IsValid)
        {
            _userService.CreateUser(user);
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }
}
```

**View (Index.cshtml)：**
```razor
@model IEnumerable<User>

<h1>Users List</h1>

<table class="table">
    <thead>
        <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var user in Model)
        {
            <tr>
                <td>@user.DisplayName</td>
                <td>@user.Email</td>
                <td>
                    <a asp-action="Details" asp-route-id="@user.Id">Details</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

**Model (User.cs)：**
```csharp
public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

### 優點
- ✅ 關注點分離（Separation of Concerns）
- ✅ 易於測試
- ✅ 完整的 HTML 控制
- ✅ 支援 RESTful 路由
- ✅ 強大的 Razor 語法

### 缺點
- ⚠️ 學習曲線較 Web Forms 陡峭
- ⚠️ 需要理解 MVC 模式

### 適用場景
- ✅ 傳統網站開發
- ✅ Server-side rendering 應用
- ✅ SEO 重要的網站
- ✅ 需要完整頁面渲染的應用

---

## 3. ASP.NET Web API

### 基本資訊
- **推出年份：** 2012
- **平台：** .NET Framework → .NET Core/.NET
- **狀態：** ✅ 非常推薦！（你的專案就是這個）
- **當前版本：** ASP.NET Core Web API

### 特色
- 🎯 專門建立 RESTful API
- 返回 JSON/XML 資料
- 不包含視圖（View）
- 輕量、高效能
- 支援內容協商（Content Negotiation）

### 範例程式碼

**Controller (UsersController.cs)：**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetUser(string id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return user;
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<AppUser>> CreateUser(AppUser user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, AppUser user)
    {
        if (id != user.Id)
            return BadRequest();

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

**返回的 JSON 格式：**
```json
// GET: api/users
[
  {
    "id": "abc-123",
    "displayName": "John Doe",
    "email": "john@example.com"
  },
  {
    "id": "def-456",
    "displayName": "Jane Smith",
    "email": "jane@example.com"
  }
]
```

### HTTP 狀態碼對應

```csharp
return Ok(data);              // 200 OK
return Created();             // 201 Created
return NoContent();           // 204 No Content
return BadRequest();          // 400 Bad Request
return Unauthorized();        // 401 Unauthorized
return NotFound();            // 404 Not Found
return StatusCode(500);       // 500 Internal Server Error
```

### 優點
- ✅ 專注於 API 開發
- ✅ 輕量、快速
- ✅ 支援多種前端（React、Vue、Angular、Mobile）
- ✅ RESTful 設計
- ✅ 易於測試
- ✅ 支援 Swagger/OpenAPI

### 適用場景
- ✅ 前後端分離應用
- ✅ Mobile App 後端
- ✅ 微服務架構
- ✅ SPA (Single Page Application)
- ✅ 你的專案！

---

## 4. ASP.NET Razor Pages

### 基本資訊
- **推出年份：** 2017
- **平台：** .NET Core/.NET
- **狀態：** ✅ 推薦新手使用
- **特色：** 頁面導向（Page-based）

### 特色
- 🎯 比 MVC 更簡單
- 每個頁面獨立（.cshtml + .cshtml.cs）
- 適合簡單網站
- MVVM 模式（Model-View-ViewModel）

### 專案結構

```
Pages/
├── Index.cshtml           (View)
├── Index.cshtml.cs        (PageModel - 邏輯)
├── Users/
│   ├── List.cshtml
│   ├── List.cshtml.cs
│   ├── Details.cshtml
│   └── Details.cshtml.cs
└── Shared/
    ├── _Layout.cshtml
    └── _ValidationScriptsPartial.cshtml
```

### 範例程式碼

**Index.cshtml.cs (PageModel)：**
```csharp
public class IndexModel : PageModel
{
    private readonly IUserService _userService;

    public IndexModel(IUserService userService)
    {
        _userService = userService;
    }

    public List<User> Users { get; set; }

    [BindProperty]
    public User NewUser { get; set; }

    public void OnGet()
    {
        Users = _userService.GetAllUsers();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        _userService.CreateUser(NewUser);
        return RedirectToPage();
    }
}
```

**Index.cshtml (View)：**
```razor
@page
@model IndexModel

<h1>Users</h1>

<form method="post">
    <input asp-for="NewUser.DisplayName" />
    <input asp-for="NewUser.Email" />
    <button type="submit">Add User</button>
</form>

<ul>
    @foreach (var user in Model.Users)
    {
        <li>@user.DisplayName - @user.Email</li>
    }
</ul>
```

### 優點
- ✅ 簡單易學
- ✅ 適合小型網站
- ✅ 減少樣板代碼
- ✅ 頁面組織清晰

### 缺點
- ⚠️ 不適合大型複雜應用
- ⚠️ 路由靈活度較 MVC 低

### 適用場景
- ✅ 中小型網站
- ✅ 後台管理系統
- ✅ 快速原型開發
- ✅ 學習 ASP.NET Core

---

## 5. Blazor

### 基本資訊
- **推出年份：** 2018
- **平台：** .NET Core/.NET
- **狀態：** ✅ 最新技術，持續發展中
- **特色：** 用 C# 寫前端！

### 兩種模式

#### 5.1 Blazor Server

**運作方式：**
```
Browser ←──SignalR──→ Server
  (UI)                (C# 執行)
```

- C# 在伺服器執行
- 透過 SignalR 即時通訊
- UI 更新透過 WebSocket

**優點：**
- ✅ 下載體積小
- ✅ 支援舊瀏覽器
- ✅ 伺服器運算能力
- ✅ 程式碼保護（不下載到客戶端）

**缺點：**
- ❌ 需要持續連線
- ❌ 延遲較高
- ❌ 伺服器負載較大

#### 5.2 Blazor WebAssembly (WASM)

**運作方式：**
```
Browser (C# → WebAssembly)
  ↓
直接在瀏覽器執行
```

- C# 編譯成 WebAssembly
- 完全在瀏覽器執行
- 類似 React/Vue，但用 C#

**優點：**
- ✅ 離線運作
- ✅ 無伺服器壓力
- ✅ 快速響應（本地執行）

**缺點：**
- ❌ 初次下載體積大（~2MB）
- ❌ 啟動較慢
- ❌ 不支援非常舊的瀏覽器

### 範例程式碼

**Counter.razor：**
```razor
@page "/counter"

<h1>Counter</h1>

<p>Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">
    Click me
</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

**FetchData.razor (呼叫 API)：**
```razor
@page "/fetchdata"
@inject HttpClient Http

<h1>Weather Forecast</h1>

@if (forecasts == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Date</th>
                <th>Temp. (C)</th>
                <th>Summary</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var forecast in forecasts)
            {
                <tr>
                    <td>@forecast.Date.ToShortDateString()</td>
                    <td>@forecast.TemperatureC</td>
                    <td>@forecast.Summary</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("api/weather");
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
    }
}
```

**元件重用 (UserCard.razor)：**
```razor
<div class="card">
    <h3>@UserName</h3>
    <p>@Email</p>
    <button @onclick="OnDelete">Delete</button>
</div>

@code {
    [Parameter]
    public string UserName { get; set; }

    [Parameter]
    public string Email { get; set; }

    [Parameter]
    public EventCallback OnDelete { get; set; }
}
```

**使用元件：**
```razor
<UserCard UserName="John" Email="john@example.com" OnDelete="HandleDelete" />
```

### 優點
- ✅ 全端使用 C#（不需寫 JavaScript）
- ✅ 共用程式碼（前後端）
- ✅ 強型別
- ✅ 現代 SPA 體驗
- ✅ 元件化開發

### 缺點
- ⚠️ 生態系不如 React/Vue 成熟
- ⚠️ 學習資源較少
- ⚠️ WASM 版本體積較大

### 適用場景
- ✅ 全端 .NET 開發團隊
- ✅ 企業內部應用
- ✅ 不想學 JavaScript 的開發者
- ✅ 需要共用邏輯的前後端

---

## 技術選擇快速指南

### 我該選擇哪個技術？

```
需求決策樹：

1. 要建立 API 給前端/Mobile 使用？
   └─ 是 → 使用 ASP.NET Web API ✅

2. 要建立傳統網站（Server-side rendering）？
   ├─ 簡單網站 → Razor Pages ✅
   └─ 複雜網站 → ASP.NET MVC ✅

3. 要建立現代 SPA，且團隊熟悉 C#？
   └─ 是 → Blazor ✅
   └─ 否 → 考慮 React/Vue + Web API

4. 維護舊系統？
   └─ Web Forms → 評估遷移或維持現狀

5. 需要同時提供網頁和 API？
   └─ MVC + Web API 混合使用 ✅
```

---

## 技術對照表

| 技術 | 年份 | 平台 | 用途 | 狀態 | 學習曲線 |
|------|------|------|------|------|---------|
| **Web Forms** | 2002 | Framework | 傳統網頁 | ❌ 過時 | 低 |
| **MVC** | 2009 | Both | Server-side 網頁 | ✅ 推薦 | 中 |
| **Web API** | 2012 | Both | RESTful API | ✅ 強推 | 中 |
| **Razor Pages** | 2017 | Core/.NET | 簡單網頁 | ✅ 推薦 | 低 |
| **Blazor** | 2018 | Core/.NET | SPA (C#) | ✅ 新技術 | 中高 |

---

## 你的專案配置

### 當前使用：
- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **MySQL**

### 架構建議：

```
┌──────────────────────────────────────┐
│         Frontend (選擇一種)           │
│  • React / Vue / Angular             │
│  • Blazor WebAssembly                │
│  • Mobile App (iOS/Android)          │
└───────────────┬──────────────────────┘
                │ HTTP / REST API
                ↓
┌──────────────────────────────────────┐
│      Backend (你的專案)               │
│  • ASP.NET Core Web API              │
│  • Controllers (UsersController...)  │
└───────────────┬──────────────────────┘
                │
                ↓
┌──────────────────────────────────────┐
│     Data Layer                       │
│  • Entity Framework Core             │
│  • ApplicationDbContext              │
└───────────────┬──────────────────────┘
                │
                ↓
┌──────────────────────────────────────┐
│          Database                    │
│  • MySQL                             │
│  • Users Table                       │
└──────────────────────────────────────┘
```

這是目前最現代、最推薦的架構！

---

## 參考資源

- [ASP.NET Core 官方文件](https://docs.microsoft.com/aspnet/core)
- [Blazor 官方文件](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [Web API 設計最佳實踐](https://docs.microsoft.com/azure/architecture/best-practices/api-design)
