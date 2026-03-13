# ASP.NET Web API 依賴注入範例

## 完整的 Repository + Service + Controller 架構

### 1. Entity (資料模型)

```csharp
// Entities/AppUser.cs
namespace API.Entities;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
```

### 2. DTOs (資料傳輸物件)

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

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
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
        return user;
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            user.IsDeleted = true; // 軟刪除
            await _context.SaveChangesAsync();
        }
    }
}
```

### 4. Service Layer

```csharp
// Services/Interfaces/IUserService.cs
using API.DTOs;

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

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
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
        // 驗證 Email 是否已存在
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
        _logger.LogInformation("User created: {UserId}", createdUser.Id);

        return MapToDto(createdUser);
    }

    public async Task UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        // 檢查新 Email 是否被其他使用者使用
        if (user.Email != dto.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
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

    private UserDto MapToDto(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email
        };
    }
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

### 6. 註冊服務 (Program.cs)

```csharp
using API.Data;
using API.Repositories.Interfaces;
using API.Repositories.Implementations;
using API.Services.Interfaces;
using API.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
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

## 測試 API

```http
### 取得所有使用者
GET https://localhost:5001/api/users

### 取得單一使用者
GET https://localhost:5001/api/users/1

### 建立使用者
POST https://localhost:5001/api/users
Content-Type: application/json

{
  "displayName": "張三",
  "email": "zhang@example.com"
}

### 更新使用者
PUT https://localhost:5001/api/users/1
Content-Type: application/json

{
  "displayName": "張三豐",
  "email": "zhang@example.com"
}

### 刪除使用者
DELETE https://localhost:5001/api/users/1
```
