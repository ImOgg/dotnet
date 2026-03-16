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

## 實際使用：三層架構中的 DI

> 完整的分層架構說明（Repository / Service / Controller 各層職責與範例）請參考 [07-service-layer-architecture.md](./07-service-layer-architecture.md)。

這裡只看 DI 在三層架構中**如何串接**的概念：

```
Program.cs（服務註冊）
    ↓
Controller（建構子注入 IUserService）
    ↓
Service（建構子注入 IUserRepository）
    ↓
Repository（建構子注入 AppDbContext）
```

每一層只依賴「介面」，不直接 `new` 下一層：

```csharp
// Controller 只認識 IUserService 介面
public class UsersController(IUserService userService) : ControllerBase { }

// Service 只認識 IUserRepository 介面
public class UserService(IUserRepository userRepository, ILogger<UserService> logger) : IUserService { }

// Repository 依賴 DbContext（由 DI 注入）
public class UserRepository(AppDbContext context) : IUserRepository { }
```

**Program.cs 註冊時，DI 容器就會自動建立整條鏈：**

```csharp
builder.Services.AddDbContext<AppDbContext>(...);               // Scoped
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Scoped
builder.Services.AddScoped<IUserService, UserService>();       // Scoped
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
