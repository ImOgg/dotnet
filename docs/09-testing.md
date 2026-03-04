# ASP.NET Core 測試完整指南

## 什麼是軟體測試?

**軟體測試**確保程式碼按預期運作,減少 bug 並提高程式碼品質。

### 測試的種類

```
測試金字塔:

        /\
       /E2E\        ← 端對端測試 (少量)
      /------\
     /整合測試 \     ← Integration Tests (中等)
    /----------\
   /  單元測試   \   ← Unit Tests (大量)
  /--------------\
```

---

## 1. 單元測試 (Unit Tests)

### 什麼是單元測試?

測試**單一方法或類別**,隔離外部依賴。

### 安裝套件

```bash
# 建立測試專案
dotnet new xunit -n API.Tests
cd API.Tests

# 安裝套件
dotnet add package xUnit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package FluentAssertions

# 加入專案參考
dotnet add reference ../API/API.csproj
```

### 基本測試結構

```csharp
// Tests/Services/UserServiceTests.cs
using Xunit;
using Moq;
using FluentAssertions;
using API.Services.Implementations;
using API.Repositories.Interfaces;
using API.DTOs;
using API.Entities;
using Microsoft.Extensions.Logging;

namespace API.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserRepo.Object,
            _mockEmailService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
    {
        // Arrange (準備)
        var userId = "123";
        var expectedUser = new AppUser
        {
            Id = userId,
            DisplayName = "Test User",
            Email = "test@example.com"
        };

        _mockUserRepo
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act (執行)
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert (驗證)
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.DisplayName.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUserByIdAsync_UserNotExists_ReturnsNull()
    {
        // Arrange
        _mockUserRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _userService.GetUserByIdAsync("999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_ValidDto_ReturnsCreatedUser()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            DisplayName = "New User",
            Email = "new@example.com"
        };

        _mockUserRepo
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null); // Email 未被使用

        _mockUserRepo
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync((AppUser user) => user);

        // Act
        var result = await _userService.CreateUserAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.DisplayName.Should().Be(dto.DisplayName);
        result.Email.Should().Be(dto.Email);

        _mockUserRepo.Verify(x => x.CreateAsync(It.IsAny<AppUser>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_EmailAlreadyExists_ThrowsException()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            DisplayName = "Test",
            Email = "existing@example.com"
        };

        _mockUserRepo
            .Setup(x => x.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new AppUser { Email = dto.Email });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.CreateUserAsync(dto));

        _mockUserRepo.Verify(x => x.CreateAsync(It.IsAny<AppUser>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetUserByIdAsync_InvalidId_ReturnsNull(string? invalidId)
    {
        // Arrange
        _mockUserRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(invalidId!);

        // Assert
        result.Should().BeNull();
    }
}
```

### 測試命名慣例

```
格式: MethodName_Scenario_ExpectedResult

範例:
- GetUserById_UserExists_ReturnsUser
- GetUserById_UserNotFound_ReturnsNull
- CreateUser_DuplicateEmail_ThrowsException
```

### 執行測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試
dotnet test --filter "FullyQualifiedName~UserServiceTests"

# 顯示詳細輸出
dotnet test -v detailed

# 產生程式碼覆蓋率報告
dotnet test /p:CollectCoverage=true
```

---

## 2. 整合測試 (Integration Tests)

### 什麼是整合測試?

測試**多個元件的整合**,包含資料庫、API 端點等。

### 安裝套件

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### 測試 WebApplicationFactory

```csharp
// Tests/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using API.Data;

namespace API.Tests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 移除原本的 DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // 加入 InMemory Database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // 初始化測試資料
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // 加入測試資料
            SeedTestData(db);
        });

        return base.CreateHost(builder);
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        context.Users.AddRange(
            new AppUser { Id = "1", DisplayName = "User 1", Email = "user1@test.com" },
            new AppUser { Id = "2", DisplayName = "User 2", Email = "user2@test.com" }
        );

        context.SaveChanges();
    }
}
```

### API 端點整合測試

```csharp
// Tests/Controllers/UsersControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using API.DTOs;
using API.Entities;

namespace API.Tests.Controllers;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users");
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUser_ExistingId_ReturnsUser()
    {
        // Act
        var response = await _client.GetAsync("/api/users/1");
        var user = await response.Content.ReadFromJsonAsync<UserDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        user.Should().NotBeNull();
        user!.Id.Should().Be("1");
    }

    [Fact]
    public async Task GetUser_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/users/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_ValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            DisplayName = "New User",
            Email = "new@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", dto);
        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        createdUser.Should().NotBeNull();
        createdUser!.DisplayName.Should().Be(dto.DisplayName);
        createdUser.Email.Should().Be(dto.Email);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            DisplayName = "Duplicate",
            Email = "user1@test.com" // 已存在
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_ExistingId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/users/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 驗證已刪除
        var getResponse = await _client.GetAsync("/api/users/1");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

---

## 3. Repository 測試

```csharp
// Tests/Repositories/UserRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using API.Data;
using API.Entities;
using API.Repositories.Implementations;

namespace API.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context, Mock.Of<ILogger<UserRepository>>());

        // 加入測試資料
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Users.AddRange(
            new AppUser { Id = "1", DisplayName = "User 1", Email = "user1@test.com" },
            new AppUser { Id = "2", DisplayName = "User 2", Email = "user2@test.com" }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Act
        var users = await _repository.GetAllAsync();

        // Assert
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        // Act
        var user = await _repository.GetByIdAsync("1");

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be("1");
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Act
        var user = await _repository.GetByEmailAsync("user1@test.com");

        // Assert
        user.Should().NotBeNull();
        user!.Email.Should().Be("user1@test.com");
    }

    [Fact]
    public async Task CreateAsync_ValidUser_CreatesUser()
    {
        // Arrange
        var newUser = new AppUser
        {
            Id = "3",
            DisplayName = "User 3",
            Email = "user3@test.com"
        };

        // Act
        var result = await _repository.CreateAsync(newUser);
        var users = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        users.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesUser()
    {
        // Act
        await _repository.DeleteAsync("1");
        var users = await _repository.GetAllAsync();

        // Assert
        users.Should().HaveCount(1);
        users.Should().NotContain(u => u.Id == "1");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

## 4. 測試最佳實踐

### AAA 模式

```csharp
[Fact]
public async Task ExampleTest()
{
    // Arrange (準備測試資料)
    var dto = new CreateUserDto { /* ... */ };

    // Act (執行要測試的方法)
    var result = await _service.CreateAsync(dto);

    // Assert (驗證結果)
    result.Should().NotBeNull();
}
```

### 使用 FluentAssertions

```csharp
// 一般 Assert
Assert.NotNull(user);
Assert.Equal("test@example.com", user.Email);

// FluentAssertions (更易讀)
user.Should().NotBeNull();
user!.Email.Should().Be("test@example.com");

// 更多範例
result.Should().BeOfType<UserDto>();
users.Should().HaveCount(5);
users.Should().Contain(u => u.Email == "test@example.com");
exception.Should().BeOfType<InvalidOperationException>();
```

### Moq 常用方法

```csharp
// Setup 回傳值
_mockRepo.Setup(x => x.GetByIdAsync("123"))
    .ReturnsAsync(new AppUser());

// Setup 回傳計算值
_mockRepo.Setup(x => x.CreateAsync(It.IsAny<AppUser>()))
    .ReturnsAsync((AppUser user) => user);

// Setup 拋出例外
_mockRepo.Setup(x => x.GetByIdAsync("999"))
    .ThrowsAsync(new KeyNotFoundException());

// Verify 方法被呼叫
_mockRepo.Verify(x => x.CreateAsync(It.IsAny<AppUser>()), Times.Once);
_mockRepo.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);

// 使用 It.Is 進行條件驗證
_mockRepo.Verify(x => x.CreateAsync(It.Is<AppUser>(u => u.Email == "test@example.com")));
```

---

## 5. 測試覆蓋率

### 安裝套件

```bash
dotnet add package coverlet.collector
```

### 產生覆蓋率報告

```bash
# 執行測試並產生覆蓋率
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# 產生 HTML 報告 (需安裝 ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage.opencover.xml" -targetdir:"coveragereport" -reporttypes:Html

# 開啟報告
start coveragereport/index.html
```

---

## 6. 完整測試專案結構

```
API.Tests/
├── Controllers/
│   ├── UsersControllerTests.cs
│   └── ProductsControllerTests.cs
├── Services/
│   ├── UserServiceTests.cs
│   └── EmailServiceTests.cs
├── Repositories/
│   └── UserRepositoryTests.cs
├── Helpers/
│   └── ValidationHelperTests.cs
├── CustomWebApplicationFactory.cs
└── API.Tests.csproj
```

---

## 7. CI/CD 整合

### GitHub Actions 範例

```yaml
# .github/workflows/test.yml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run tests
      run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true

    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

---

## 總結

這份指南涵蓋了:
- ✅ 單元測試 (使用 xUnit, Moq, FluentAssertions)
- ✅ 整合測試 (使用 WebApplicationFactory)
- ✅ Repository 測試 (使用 InMemory Database)
- ✅ 測試最佳實踐 (AAA 模式)
- ✅ 測試覆蓋率報告
- ✅ CI/CD 整合

### 測試原則

1. **F.I.R.S.T 原則**
   - **F**ast - 快速執行
   - **I**ndependent - 測試之間獨立
   - **R**epeatable - 可重複執行
   - **S**elf-Validating - 自動驗證結果
   - **T**imely - 及時撰寫測試

2. **測試覆蓋率目標**
   - Service 層: 80%+
   - Repository 層: 70%+
   - Controller 層: 60%+

3. **優先測試**
   - 核心業務邏輯
   - 容易出錯的部分
   - 複雜的演算法
