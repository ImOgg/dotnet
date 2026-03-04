# 依賴注入 (Dependency Injection) 完整指南

## 什麼是依賴注入？

**依賴注入 (DI)** 是一種設計模式,用於實現**控制反轉 (IoC)**。

### 簡單比喻

```
❌ 不好的做法 (沒有 DI):
class Car {
    private Engine engine = new Engine(); // 自己建立依賴
}

✅ 好的做法 (使用 DI):
class Car {
    private Engine engine;
    public Car(Engine engine) {  // 從外部注入依賴
        this.engine = engine;
    }
}
```

### 為什麼需要 DI？

1. **降低耦合度** - 類別不需要知道如何建立依賴物件
2. **提高可測試性** - 容易替換成 Mock 物件進行測試
3. **提高可維護性** - 修改實作不影響使用者
4. **提高可重用性** - 相同介面可以有不同實作

---

## ASP.NET Core 內建的 DI 容器

ASP.NET Core 有內建的 DI 容器,不需要額外安裝套件。

### 服務生命週期

| 生命週期 | 說明 | 何時使用 |
|---------|------|---------|
| **Transient** | 每次請求都建立新實例 | 輕量、無狀態的服務 |
| **Scoped** | 每個 HTTP 請求建立一個實例 | DbContext、Repository |
| **Singleton** | 應用程式啟動時建立一次,全域共用 | 設定、快取、工具類別 |

### 生命週期圖解

```
Transient (每次都是新的):
Request 1: Service-A1, Service-A2, Service-A3
Request 2: Service-B1, Service-B2, Service-B3

Scoped (同一個請求內共用):
Request 1: Service-A (共用)
Request 2: Service-B (共用)

Singleton (全域共用):
Request 1: Service-X (共用)
Request 2: Service-X (同一個實例)
Request 3: Service-X (同一個實例)
```

---

## 1. 基本使用

### 定義介面與實作

```csharp
// Services/IGreetingService.cs
namespace API.Services;

public interface IGreetingService
{
    string GetGreeting(string name);
}
```

```csharp
// Services/GreetingService.cs
namespace API.Services;

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

### 註冊服務

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 註冊服務 - Transient
builder.Services.AddTransient<IGreetingService, GreetingService>();

var app = builder.Build();
```

### 在 Controller 中使用

```csharp
// Controllers/GreetingController.cs
using Microsoft.AspNetCore.Mvc;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreetingController : ControllerBase
{
    private readonly IGreetingService _greetingService;

    // 透過建構子注入
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

## 2. 三種生命週期詳解

### Transient - 暫時性

每次請求服務時都建立新實例。

```csharp
// Program.cs
builder.Services.AddTransient<ITransientService, TransientService>();
```

**適用場景:**
- 輕量級、無狀態的服務
- 工具類別、Helper 類別

**範例:**
```csharp
// Services/GuidService.cs
public interface IGuidService
{
    Guid GetGuid();
}

public class GuidService : IGuidService
{
    private readonly Guid _guid;

    public GuidService()
    {
        _guid = Guid.NewGuid();
    }

    public Guid GetGuid() => _guid;
}

// 註冊
builder.Services.AddTransient<IGuidService, GuidService>();

// 使用
[HttpGet("transient-test")]
public IActionResult TestTransient(
    [FromServices] IGuidService service1,
    [FromServices] IGuidService service2)
{
    return Ok(new
    {
        guid1 = service1.GetGuid(), // 不同的 GUID
        guid2 = service2.GetGuid(), // 不同的 GUID
        areEqual = service1.GetGuid() == service2.GetGuid() // False
    });
}
```

### Scoped - 範圍性

每個 HTTP 請求建立一個實例,在同一個請求內共用。

```csharp
// Program.cs
builder.Services.AddScoped<IScopedService, ScopedService>();
```

**適用場景:**
- **Entity Framework DbContext** (最重要!)
- Repository 模式
- 需要在同一個請求中共用狀態的服務

**範例:**
```csharp
// 註冊 DbContext (預設是 Scoped)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Repository 也註冊為 Scoped
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 使用
[HttpGet("scoped-test")]
public IActionResult TestScoped(
    [FromServices] IGuidService service1,
    [FromServices] IGuidService service2)
{
    return Ok(new
    {
        guid1 = service1.GetGuid(), // 相同的 GUID
        guid2 = service2.GetGuid(), // 相同的 GUID
        areEqual = service1.GetGuid() == service2.GetGuid() // True
    });
}
```

### Singleton - 單例

應用程式啟動時建立一次,全域共用同一個實例。

```csharp
// Program.cs
builder.Services.AddSingleton<ISingletonService, SingletonService>();
```

**適用場景:**
- 設定物件 (AppSettings)
- 快取服務
- 日誌服務
- 無狀態的工具類別

**範例:**
```csharp
// Services/CounterService.cs
public interface ICounterService
{
    int Increment();
    int GetCount();
}

public class CounterService : ICounterService
{
    private int _count = 0;

    public int Increment() => Interlocked.Increment(ref _count);
    public int GetCount() => _count;
}

// 註冊
builder.Services.AddSingleton<ICounterService, CounterService>();

// 使用
[HttpGet("counter")]
public IActionResult GetCounter([FromServices] ICounterService counter)
{
    counter.Increment();
    return Ok(new { count = counter.GetCount() }); // 每次呼叫都會累加
}
```

---

## 3. 實際應用範例

### Repository 模式 + DI

```csharp
// Repositories/IUserRepository.cs
namespace API.Repositories;

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
// Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;

namespace API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        ApplicationDbContext context,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AppUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<AppUser?> GetByIdAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
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
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User deleted: {UserId}", id);
        }
    }
}
```

### Service 層

```csharp
// Services/IUserService.cs
namespace API.Services;

public interface IUserService
{
    Task<List<AppUser>> GetAllUsersAsync();
    Task<AppUser?> GetUserByIdAsync(string id);
    Task<AppUser> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(string id, UpdateUserDto dto);
    Task DeleteUserAsync(string id);
}
```

```csharp
// Services/UserService.cs
using API.Repositories;
using API.DTOs;
using API.Entities;

namespace API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<AppUser>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(string id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<AppUser> CreateUserAsync(CreateUserDto dto)
    {
        // 檢查 Email 是否已存在
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email 已被使用");
        }

        var user = new AppUser
        {
            DisplayName = dto.DisplayName,
            Email = dto.Email
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // 發送歡迎信 (背景工作)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "歡迎加入",
                    $"Hi {user.DisplayName}, 歡迎!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email");
            }
        });

        return createdUser;
    }

    public async Task UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        user.DisplayName = dto.DisplayName;
        user.Email = dto.Email;

        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(string id)
    {
        await _userRepository.DeleteAsync(id);
    }
}
```

### 註冊所有服務

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// DbContext (Scoped)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Repositories (Scoped - 因為依賴 DbContext)
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services (Scoped)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Settings (Singleton)
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();
```

### Controller 使用

```csharp
// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST: api/users
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

    // PUT: api/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, UpdateUserDto dto)
    {
        try
        {
            await _userService.UpdateUserAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
```

---

## 4. 進階技巧

### 多個實作的選擇

```csharp
// Services/IPaymentService.cs
public interface IPaymentService
{
    Task ProcessPaymentAsync(decimal amount);
}

public class CreditCardPaymentService : IPaymentService
{
    public Task ProcessPaymentAsync(decimal amount)
    {
        // 信用卡支付邏輯
        return Task.CompletedTask;
    }
}

public class PayPalPaymentService : IPaymentService
{
    public Task ProcessPaymentAsync(decimal amount)
    {
        // PayPal 支付邏輯
        return Task.CompletedTask;
    }
}

// 註冊多個實作
builder.Services.AddKeyedScoped<IPaymentService, CreditCardPaymentService>("CreditCard");
builder.Services.AddKeyedScoped<IPaymentService, PayPalPaymentService>("PayPal");

// 使用
public class OrderService
{
    private readonly IPaymentService _creditCardPayment;
    private readonly IPaymentService _paypalPayment;

    public OrderService(
        [FromKeyedServices("CreditCard")] IPaymentService creditCardPayment,
        [FromKeyedServices("PayPal")] IPaymentService paypalPayment)
    {
        _creditCardPayment = creditCardPayment;
        _paypalPayment = paypalPayment;
    }
}
```

### 工廠模式 + DI

```csharp
// Services/IPaymentServiceFactory.cs
public interface IPaymentServiceFactory
{
    IPaymentService GetPaymentService(string paymentMethod);
}

public class PaymentServiceFactory : IPaymentServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentService GetPaymentService(string paymentMethod)
    {
        return paymentMethod switch
        {
            "CreditCard" => _serviceProvider.GetRequiredKeyedService<IPaymentService>("CreditCard"),
            "PayPal" => _serviceProvider.GetRequiredKeyedService<IPaymentService>("PayPal"),
            _ => throw new ArgumentException("Unsupported payment method")
        };
    }
}

// 註冊
builder.Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();
```

### 條件式註冊

```csharp
// 根據環境註冊不同的服務
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, EmailService>();
}
```

---

## 5. 服務註冊的常用擴展方法

### 批量註冊服務

```csharp
// Extensions/ServiceCollectionExtensions.cs
namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                config.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(config.GetConnectionString("DefaultConnection"))
            ));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Settings
        services.Configure<EmailSettings>(
            config.GetSection("EmailSettings"));

        return services;
    }
}
```

```csharp
// Program.cs
using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 使用擴展方法一次註冊所有服務
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();
```

---

## 6. 常見錯誤與解決方法

### ❌ 錯誤 1: 生命週期不匹配

```csharp
// ❌ 錯誤: Singleton 依賴 Scoped
builder.Services.AddSingleton<SingletonService>(); // 依賴 DbContext
builder.Services.AddScoped<ApplicationDbContext>();

// ✅ 解決: 改用 IServiceProvider 手動建立 Scope
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

### ❌ 錯誤 2: 忘記註冊服務

```csharp
// 執行時會拋出異常:
// Unable to resolve service for type 'IUserService'

// ✅ 解決: 確保已註冊
builder.Services.AddScoped<IUserService, UserService>();
```

### ❌ 錯誤 3: 循環依賴

```csharp
// ❌ 錯誤: A → B → A
public class ServiceA
{
    public ServiceA(ServiceB b) { }
}

public class ServiceB
{
    public ServiceB(ServiceA a) { }  // 循環依賴!
}

// ✅ 解決: 重新設計,打破循環
// 方法 1: 抽取共用介面
// 方法 2: 使用事件/訊息機制
// 方法 3: 使用 Lazy<T>
```

---

## 最佳實踐

### ✅ 應該做的

1. **優先使用介面** - 而不是具體類別
2. **選擇正確的生命週期**
   - DbContext → Scoped
   - Repository → Scoped
   - Service → Scoped
   - Settings → Singleton
3. **使用建構子注入** - 避免屬性注入
4. **避免 Service Locator 模式** - 不要直接注入 `IServiceProvider`
5. **使用擴展方法組織註冊** - 保持 Program.cs 整潔

### ❌ 不應該做的

1. ❌ 在 Singleton 中注入 Scoped 服務
2. ❌ 建立循環依賴
3. ❌ 過度使用 DI (簡單的值物件不需要)
4. ❌ 在 Controller 中直接使用 Repository

---

## 總結

這份指南涵蓋了:
- ✅ DI 的基本概念
- ✅ 三種服務生命週期 (Transient、Scoped、Singleton)
- ✅ Repository 模式 + Service 層完整範例
- ✅ 進階技巧 (工廠模式、多實作選擇)
- ✅ 常見錯誤與解決方法
- ✅ 最佳實踐

### 推薦的專案架構

```
API/
├── Controllers/          # API 端點
├── Services/            # 業務邏輯
│   ├── Interfaces/
│   └── Implementations/
├── Repositories/        # 資料存取
│   ├── Interfaces/
│   └── Implementations/
├── Data/               # DbContext
├── Entities/           # 資料模型
├── DTOs/               # 資料傳輸物件
├── Settings/           # 設定類別
└── Extensions/         # 擴展方法
    └── ServiceCollectionExtensions.cs
```
