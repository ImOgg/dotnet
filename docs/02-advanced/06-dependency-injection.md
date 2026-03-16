# 依賴注入 (Dependency Injection)

> 一句話摘要：ASP.NET Core 內建 DI 容器，透過建構子注入實現控制反轉，降低耦合度並提高可測試性。

## 核心概念

### 什麼是依賴注入？

```
❌ 沒有 DI（自己建立依賴）:
class Car {
    private Engine engine = new Engine();
}

✅ 使用 DI（從外部注入依賴）:
class Car {
    private Engine engine;
    public Car(Engine engine) {
        this.engine = engine;
    }
}
```

### 為什麼需要 DI？

- **降低耦合度** - 類別不需要知道如何建立依賴物件
- **提高可測試性** - 容易替換成 Mock 物件
- **提高可維護性** - 修改實作不影響使用者
- **提高可重用性** - 相同介面可以有不同實作

---

## 服務生命週期

| 生命週期 | 說明 | 適用場景 |
|---------|------|---------|
| **Transient** | 每次注入都建立新實例 | 輕量、無狀態的工具類別 |
| **Scoped** | 每個 HTTP 請求建立一個實例 | DbContext、Repository、Service |
| **Singleton** | 應用程式啟動時建立一次 | 設定、快取、無狀態工具類別 |

```
Transient: Request 1 → A1, A2, A3 (每次都不同)
Scoped:    Request 1 → A (共用), Request 2 → B (共用)
Singleton: 所有請求 → X (永遠同一個)
```

### Transient 範例

```csharp
public interface IGuidService { Guid GetGuid(); }

public class GuidService : IGuidService
{
    private readonly Guid _guid = Guid.NewGuid();
    public Guid GetGuid() => _guid;
}

// 註冊
builder.Services.AddTransient<IGuidService, GuidService>();

// 測試：兩個注入的實例 GUID 不同
[HttpGet("transient-test")]
public IActionResult TestTransient(
    [FromServices] IGuidService s1,
    [FromServices] IGuidService s2)
{
    return Ok(new { guid1 = s1.GetGuid(), guid2 = s2.GetGuid(), areEqual = false });
}
```

### Singleton 範例

```csharp
public interface ICounterService { int Increment(); int GetCount(); }

public class CounterService : ICounterService
{
    private int _count = 0;
    public int Increment() => Interlocked.Increment(ref _count);
    public int GetCount() => _count;
}

// 註冊
builder.Services.AddSingleton<ICounterService, CounterService>();

// 每次呼叫都會累加，所有請求共用同一個計數器
[HttpGet("counter")]
public IActionResult GetCounter([FromServices] ICounterService counter)
{
    counter.Increment();
    return Ok(new { count = counter.GetCount() });
}
```

---

## 基本使用

### 定義介面與實作

```csharp
// Services/IGreetingService.cs
public interface IGreetingService
{
    string GetGreeting(string name);
}

// Services/GreetingService.cs
public class GreetingService : IGreetingService
{
    private readonly ILogger<GreetingService> _logger;

    public GreetingService(ILogger<GreetingService> logger)
    {
        _logger = logger;
    }

    public string GetGreeting(string name)
    {
        _logger.LogInformation("Greeting {Name}", name);
        return $"Hello, {name}!";
    }
}
```

### 註冊與使用

```csharp
// Program.cs
builder.Services.AddTransient<IGreetingService, GreetingService>();
```

```csharp
// Controllers/GreetingController.cs
[ApiController]
[Route("api/[controller]")]
public class GreetingController : ControllerBase
{
    private readonly IGreetingService _greetingService;

    public GreetingController(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    [HttpGet("{name}")]
    public IActionResult GetGreeting(string name)
    {
        var greeting = _greetingService.GetGreeting(name);
        return Ok(new { message = greeting });
    }
}
```

---

## Repository + Service + Controller 完整範例

### 1. Entity

```csharp
// Entities/AppUser.cs
namespace API.Entities;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;  // 軟刪除標記
}
```

### 2. DTOs

```csharp
// DTOs/UserDtos.cs
namespace API.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateUserDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### 3. Repository Layer

```csharp
// Repositories/Interfaces/IUserRepository.cs
namespace API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser> CreateAsync(AppUser user);
    Task UpdateAsync(AppUser user);
    Task DeleteAsync(string id);
}
```

```csharp
// Repositories/Implementations/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AppUser>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<AppUser?> GetByIdAsync(string id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User created: {UserId}", user.Id);
        return user;
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User updated: {UserId}", user.Id);
    }

    public async Task DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;  // 軟刪除，不實際移除資料
            await _context.SaveChangesAsync();
            _logger.LogInformation("User deleted: {UserId}", id);
        }
    }
}
```

### 4. Service Layer

```csharp
// Services/Interfaces/IUserService.cs
namespace API.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(string id, UpdateUserDto dto);
    Task DeleteUserAsync(string id);
}
```

```csharp
// Services/Implementations/UserService.cs
using API.DTOs;
using API.Entities;
using API.Repositories.Interfaces;
using API.Services.Interfaces;

namespace API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email 已被使用");

        var user = new AppUser { DisplayName = dto.DisplayName, Email = dto.Email };
        var createdUser = await _userRepository.CreateAsync(user);
        _logger.LogInformation("User created: {UserId}", createdUser.Id);

        return MapToDto(createdUser);
    }

    public async Task UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("找不到使用者");

        if (user.Email != dto.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email 已被使用");
        }

        user.DisplayName = dto.DisplayName;
        user.Email = dto.Email;

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User updated: {UserId}", id);
    }

    public async Task DeleteUserAsync(string id)
    {
        await _userRepository.DeleteAsync(id);
        _logger.LogInformation("User deleted: {UserId}", id);
    }

    private static UserDto MapToDto(AppUser user) => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email
    };
}
```

### 5. Controller

```csharp
// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { error = "找不到使用者" });

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, UpdateUserDto dto)
    {
        try
        {
            await _userService.UpdateUserAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
```

### 6. Program.cs

```csharp
using API.Data;
using API.Repositories.Interfaces;
using API.Repositories.Implementations;
using API.Services.Interfaces;
using API.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext（Scoped）
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repositories（Scoped，依賴 DbContext）
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services（Scoped）
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### 7. 測試 API

```http
### 取得所有使用者
GET https://localhost:5001/api/users

### 取得單一使用者
GET https://localhost:5001/api/users/{id}

### 建立使用者
POST https://localhost:5001/api/users
Content-Type: application/json

{
  "displayName": "張三",
  "email": "zhang@example.com"
}

### 更新使用者
PUT https://localhost:5001/api/users/{id}
Content-Type: application/json

{
  "displayName": "張三豐",
  "email": "zhang@example.com"
}

### 刪除使用者（軟刪除）
DELETE https://localhost:5001/api/users/{id}
```

---

## 進階技巧

### 批量註冊（Extension Methods）

隨著服務增多，建議用擴展方法整理 `Program.cs`：

```csharp
// Extensions/ServiceCollectionExtensions.cs
namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                config.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection"))
            ));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();

        // Settings
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));

        return services;
    }
}
```

```csharp
// Program.cs
using API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder.Configuration);
```

### 多個實作（Keyed Services）

```csharp
// 註冊多個實作
builder.Services.AddKeyedScoped<IPaymentService, CreditCardPaymentService>("CreditCard");
builder.Services.AddKeyedScoped<IPaymentService, PayPalPaymentService>("PayPal");

// 使用
public class OrderService
{
    public OrderService(
        [FromKeyedServices("CreditCard")] IPaymentService creditCardPayment,
        [FromKeyedServices("PayPal")] IPaymentService paypalPayment) { }
}
```

### 工廠模式

```csharp
public class PaymentServiceFactory : IPaymentServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentService GetPaymentService(string method) => method switch
    {
        "CreditCard" => _serviceProvider.GetRequiredKeyedService<IPaymentService>("CreditCard"),
        "PayPal" => _serviceProvider.GetRequiredKeyedService<IPaymentService>("PayPal"),
        _ => throw new ArgumentException("Unsupported payment method")
    };
}
```

### 條件式註冊

```csharp
// 開發環境用假的 Email，正式環境才真的發信
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
else
    builder.Services.AddScoped<IEmailService, EmailService>();
```

---

## 常見錯誤

### Singleton 依賴 Scoped 服務

```csharp
// ❌ 錯誤：Singleton 不能直接依賴 Scoped 的 DbContext
builder.Services.AddSingleton<SingletonService>();
builder.Services.AddScoped<ApplicationDbContext>();

// ✅ 解決：手動建立 Scope
public class SingletonService
{
    private readonly IServiceProvider _serviceProvider;

    public SingletonService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void DoWork()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // 使用 context
    }
}
```

### 忘記註冊服務

```
// 執行時拋出：Unable to resolve service for type 'IUserService'
// 解決：確保在 Program.cs 加上：
builder.Services.AddScoped<IUserService, UserService>();
```

### 循環依賴

```csharp
// ❌ A → B → A，會拋出例外
public class ServiceA { public ServiceA(ServiceB b) { } }
public class ServiceB { public ServiceB(ServiceA a) { } }

// ✅ 解決方案：
// 1. 抽取共用介面或第三個服務
// 2. 使用事件 / 訊息機制解耦
// 3. 使用 Lazy<T> 延遲注入
```

---

## 最佳實踐

**應該做的：**
- 優先注入介面，而非具體類別
- DbContext / Repository / Service 使用 Scoped
- 設定物件、快取服務使用 Singleton
- 使用建構子注入，避免屬性注入
- 用擴展方法整理 Program.cs

**不應該做的：**
- 在 Singleton 中直接注入 Scoped 服務
- 建立循環依賴
- 在 Controller 中直接使用 Repository（跳過 Service 層）
- 濫用 `IServiceProvider`（Service Locator 反模式）

---

## 推薦專案架構

```
API/
├── Controllers/          # API 端點
├── Services/             # 業務邏輯
│   ├── Interfaces/
│   └── Implementations/
├── Repositories/         # 資料存取
│   ├── Interfaces/
│   └── Implementations/
├── Data/                 # DbContext
├── Entities/             # 資料模型
├── DTOs/                 # 資料傳輸物件
├── Settings/             # 設定類別
└── Extensions/           # 擴展方法
    └── ServiceCollectionExtensions.cs
```
