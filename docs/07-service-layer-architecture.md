# 服務層架構與方法抽離完整指南

## 什麼是分層架構？

**分層架構**將應用程式分成不同的層級,每一層都有明確的職責。

### 常見的分層

```
┌─────────────────────┐
│   Presentation      │  ← Controllers (API 端點)
├─────────────────────┤
│   Business Logic    │  ← Services (業務邏輯)
├─────────────────────┤
│   Data Access       │  ← Repositories (資料存取)
├─────────────────────┤
│   Database          │  ← MySQL, SQL Server, etc.
└─────────────────────┘
```

### 為什麼要分層?

1. **關注點分離** - 每一層只專注於自己的職責
2. **提高可測試性** - 容易進行單元測試
3. **提高可維護性** - 修改某一層不影響其他層
4. **提高可重用性** - 相同的服務可以在不同的 Controller 使用

---

## 1. 專案結構

### 推薦的資料夾結構

```
API/
├── Controllers/              # API 端點 (Presentation Layer)
│   ├── UsersController.cs
│   ├── ProductsController.cs
│   └── OrdersController.cs
│
├── Services/                 # 業務邏輯層 (Business Logic Layer)
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   ├── IProductService.cs
│   │   └── IOrderService.cs
│   └── Implementations/
│       ├── UserService.cs
│       ├── ProductService.cs
│       └── OrderService.cs
│
├── Repositories/             # 資料存取層 (Data Access Layer)
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   ├── IProductRepository.cs
│   │   └── IOrderRepository.cs
│   └── Implementations/
│       ├── UserRepository.cs
│       ├── ProductRepository.cs
│       └── OrderRepository.cs
│
├── Data/                     # 資料庫相關
│   ├── ApplicationDbContext.cs
│   └── Configurations/       # Entity 設定
│       ├── UserConfiguration.cs
│       └── ProductConfiguration.cs
│
├── Entities/                 # 資料模型 (Domain Models)
│   ├── AppUser.cs
│   ├── Product.cs
│   └── Order.cs
│
├── DTOs/                     # 資料傳輸物件
│   ├── UserDtos.cs
│   ├── ProductDtos.cs
│   └── OrderDtos.cs
│
├── Helpers/                  # 工具類別
│   ├── MappingProfiles.cs    # AutoMapper 設定
│   ├── ValidationHelper.cs
│   └── StringHelper.cs
│
├── Extensions/               # 擴展方法
│   ├── ServiceCollectionExtensions.cs
│   └── StringExtensions.cs
│
├── Settings/                 # 設定類別
│   ├── EmailSettings.cs
│   └── JwtSettings.cs
│
└── Middleware/               # 中介軟體
    ├── ErrorHandlingMiddleware.cs
    └── LoggingMiddleware.cs
```

---

## 2. 各層職責詳解

### Controller 層 (Presentation)

**職責:**
- 接收 HTTP 請求
- 驗證輸入參數 (基本驗證)
- 呼叫 Service 層
- 返回 HTTP 回應

**❌ 不應該做的:**
- 包含業務邏輯
- 直接存取 Repository 或 DbContext
- 複雜的資料處理

```csharp
// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using API.Services.Interfaces;
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
            return NotFound(new { error = "找不到使用者" });

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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // DELETE: api/users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
```

### Service 層 (Business Logic)

**職責:**
- 實作業務邏輯
- 驗證業務規則
- 協調多個 Repository
- 處理交易 (Transaction)

**❌ 不應該做的:**
- 直接處理 HTTP 請求/回應
- 直接使用 Entity Framework (應該透過 Repository)

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
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<bool> IsEmailAvailableAsync(string email);
}
```

```csharp
// Services/Implementations/UserService.cs
using API.Services.Interfaces;
using API.Repositories.Interfaces;
using API.DTOs;
using API.Entities;

namespace API.Services.Implementations;

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

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            DisplayName = u.DisplayName,
            Email = u.Email
        }).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // 業務規則驗證
        if (!await IsEmailAvailableAsync(dto.Email))
        {
            throw new InvalidOperationException("Email 已被使用");
        }

        // 建立使用者
        var user = new AppUser
        {
            DisplayName = dto.DisplayName,
            Email = dto.Email
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // 發送歡迎信 (非同步,不阻塞)
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
                _logger.LogError(ex, "Error sending welcome email to {Email}", user.Email);
            }
        });

        _logger.LogInformation("User created: {UserId}, {Email}", user.Id, user.Email);

        return new UserDto
        {
            Id = createdUser.Id,
            DisplayName = createdUser.DisplayName,
            Email = createdUser.Email
        };
    }

    public async Task UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        // 如果 Email 有變更,檢查是否可用
        if (user.Email != dto.Email)
        {
            if (!await IsEmailAvailableAsync(dto.Email))
            {
                throw new InvalidOperationException("Email 已被使用");
            }
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

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email
        };
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user == null;
    }
}
```

### Repository 層 (Data Access)

**職責:**
- 封裝資料存取邏輯
- CRUD 操作
- 查詢資料庫

**❌ 不應該做的:**
- 包含業務邏輯
- 回傳 DTO (應該回傳 Entity)

```csharp
// Repositories/Interfaces/IUserRepository.cs
using API.Entities;

namespace API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(string id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser> CreateAsync(AppUser user);
    Task UpdateAsync(AppUser user);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
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

    public UserRepository(
        ApplicationDbContext context,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AppUser>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
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
        return user;
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }
}
```

---

## 3. 通用 Repository 模式

### 建立 Generic Repository

```csharp
// Repositories/Interfaces/IRepository.cs
namespace API.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<bool> ExistsAsync(object id);
}
```

```csharp
// Repositories/Implementations/Repository.cs
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Repositories.Interfaces;

namespace API.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(object id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(object id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity != null;
    }
}
```

### 使用 Generic Repository

```csharp
// Repositories/Interfaces/IUserRepository.cs
namespace API.Repositories.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<List<AppUser>> GetActiveUsersAsync();
}
```

```csharp
// Repositories/Implementations/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories.Implementations;

public class UserRepository : Repository<AppUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<AppUser>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }
}
```

---

## 4. Helper 與 Extension 方法

### 字串驗證 Helper

```csharp
// Helpers/ValidationHelper.cs
namespace API.Helpers;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10;
    }

    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
```

### 字串擴展方法

```csharp
// Extensions/StringExtensions.cs
namespace API.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return value.Length <= maxLength
            ? value
            : value.Substring(0, maxLength) + "...";
    }

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLower());
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}
```

### 使用範例

```csharp
// 在 Service 中使用
public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
{
    // 使用 ValidationHelper
    if (!ValidationHelper.IsValidEmail(dto.Email))
    {
        throw new InvalidOperationException("Email 格式不正確");
    }

    // 使用 StringExtensions
    var displayName = dto.DisplayName.ToTitleCase();

    var user = new AppUser
    {
        DisplayName = displayName,
        Email = dto.Email
    };

    return await _userRepository.CreateAsync(user);
}
```

---

## 5. 服務註冊整理

### 使用擴展方法統一管理

```csharp
// Extensions/ServiceCollectionExtensions.cs
using API.Data;
using API.Services.Interfaces;
using API.Services.Implementations;
using API.Repositories.Interfaces;
using API.Repositories.Implementations;
using API.Settings;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // DbContext
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Settings
        services.Configure<EmailSettings>(
            config.GetSection("EmailSettings"));
        services.Configure<JwtSettings>(
            config.GetSection("JwtSettings"));

        return services;
    }

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services)
    {
        // Background Services
        services.AddHostedService<DataCleanupService>();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        return services;
    }
}
```

### 在 Program.cs 中使用

```csharp
// Program.cs
using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 使用擴展方法註冊服務
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddBackgroundServices();
builder.Services.AddCorsPolicy();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 6. 實際應用範例: 訂單系統

### Entities

```csharp
// Entities/Order.cs
namespace API.Entities;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Delivered
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AppUser? User { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
```

### DTOs

```csharp
// DTOs/OrderDtos.cs
namespace API.DTOs;

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDetailDto> Items { get; set; } = new();
}

public class OrderItemDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

### Repository

```csharp
// Repositories/Interfaces/IOrderRepository.cs
namespace API.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetUserOrdersAsync(string userId);
    Task<Order?> GetOrderWithItemsAsync(int orderId);
}

// Repositories/Implementations/OrderRepository.cs
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;

namespace API.Repositories.Implementations;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetUserOrdersAsync(string userId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
```

### Service

```csharp
// Services/Interfaces/IOrderService.cs
namespace API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto);
    Task<List<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task UpdateOrderStatusAsync(int orderId, string status);
}

// Services/Implementations/OrderService.cs
using API.Repositories.Interfaces;
using API.DTOs;
using API.Entities;

namespace API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto)
    {
        // 驗證商品是否存在並計算總金額
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"找不到商品 ID: {item.ProductId}");
            }

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.Price
            });

            totalAmount += product.Price * item.Quantity;
        }

        // 建立訂單
        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalAmount,
            Status = "Pending",
            Items = orderItems
        };

        var createdOrder = await _orderRepository.CreateAsync(order);

        // 發送確認信
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "user@example.com", // 實際應該從 UserRepository 取得
                    "訂單已建立",
                    $"您的訂單 #{createdOrder.Id} 已成功建立,總金額: ${totalAmount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order confirmation email");
            }
        });

        _logger.LogInformation("Order created: {OrderId}, Total: {Total}", createdOrder.Id, totalAmount);

        return MapToDto(createdOrder);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _orderRepository.GetUserOrdersAsync(userId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        return order == null ? null : MapToDto(order);
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("找不到訂單");
        }

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Order status updated: {OrderId}, Status: {Status}", orderId, status);
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDetailDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };
    }
}
```

---

## 總結

這份指南涵蓋了:
- ✅ 完整的分層架構 (Controller → Service → Repository)
- ✅ 各層的職責劃分
- ✅ Generic Repository 模式
- ✅ Helper 與 Extension 方法
- ✅ 服務註冊的統一管理
- ✅ 實際應用範例 (訂單系統)

### 最佳實踐

1. **Controller 只負責 HTTP,不包含業務邏輯**
2. **Service 層實作所有業務邏輯**
3. **Repository 只負責資料存取**
4. **使用 DI 進行鬆耦合**
5. **使用 DTO 進行資料傳輸**
6. **使用擴展方法組織服務註冊**
