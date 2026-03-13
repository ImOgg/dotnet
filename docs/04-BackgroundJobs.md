# ASP.NET Web API 背景工作範例

## 1. 使用 IHostedService 建立定期任務

```csharp
// Services/DataCleanupService.cs
namespace API.Services;

public class DataCleanupService : BackgroundService
{
    private readonly ILogger<DataCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DataCleanupService(
        ILogger<DataCleanupService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                    // 刪除 30 天前的軟刪除資料
                    var cutoffDate = DateTime.UtcNow.AddDays(-30);
                    var expiredUsers = await dbContext.Users
                        .Where(u => u.IsDeleted && u.CreatedAt < cutoffDate)
                        .ToListAsync();

                    dbContext.Users.RemoveRange(expiredUsers);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Cleaned up {Count} expired users",
                        expiredUsers.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup service");
            }

            // 每小時執行一次
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

## 2. 背景任務佇列

```csharp
// Services/BackgroundTaskQueue.cs
using System.Threading.Channels;

namespace API.Services;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ValueTask> workItem)
    {
        if (workItem == null)
            throw new ArgumentNullException(nameof(workItem));

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
```

```csharp
// Services/QueuedHostedService.cs
namespace API.Services;

public class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is running");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing work item");
            }
        }
    }
}
```

## 3. 匯入服務範例

```csharp
// Services/ImportService.cs
namespace API.Services;

public interface IImportService
{
    Task QueueImportJobAsync(string filePath);
}

public class ImportService : IImportService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        IBackgroundTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<ImportService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task QueueImportJobAsync(string filePath)
    {
        await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
        {
            await ProcessImportAsync(filePath, token);
        });
    }

    private async Task ProcessImportAsync(string filePath, CancellationToken token)
    {
        _logger.LogInformation("Starting import from {FilePath}", filePath);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var lines = await File.ReadAllLinesAsync(filePath, token);
        var users = new List<AppUser>();

        foreach (var line in lines.Skip(1)) // 跳過標題行
        {
            if (token.IsCancellationRequested)
                break;

            var parts = line.Split(',');
            if (parts.Length >= 2)
            {
                users.Add(new AppUser
                {
                    DisplayName = parts[0].Trim(),
                    Email = parts[1].Trim()
                });
            }
        }

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync(token);

        _logger.LogInformation(
            "Import completed. {Count} users imported",
            users.Count);

        // 刪除臨時檔案
        File.Delete(filePath);
    }
}
```

## 4. 在 Controller 中使用

```csharp
// Controllers/ImportController.cs
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly IWebHostEnvironment _environment;

    public ImportController(
        IImportService importService,
        IWebHostEnvironment environment)
    {
        _importService = importService;
        _environment = environment;
    }

    [HttpPost("users")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "請選擇 CSV 檔案" });

        if (!file.FileName.EndsWith(".csv"))
            return BadRequest(new { error = "只接受 CSV 檔案" });

        // 儲存檔案
        var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "temp");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}.csv";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 加入背景處理佇列
        await _importService.QueueImportJobAsync(filePath);

        return Accepted(new
        {
            message = "檔案已接收,正在背景處理中",
            fileName = file.FileName
        });
    }
}
```

## 5. 註冊服務

```csharp
// Program.cs
using API.Services;

var builder = WebApplication.CreateBuilder(args);

// 註冊背景任務佇列
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IImportService, ImportService>();

// 註冊 Hosted Services
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHostedService<DataCleanupService>();

var app = builder.Build();
app.Run();
```

## 6. 測試範例

### 建立測試 CSV 檔案 (users.csv)
```csv
DisplayName,Email
張三,zhang@example.com
李四,li@example.com
王五,wang@example.com
```

### 上傳並匯入
```http
POST https://localhost:5001/api/import/users
Content-Type: multipart/form-data

file: users.csv
```

### 回應
```json
{
  "message": "檔案已接收,正在背景處理中",
  "fileName": "users.csv"
}
```

## 7. 監控背景工作

```csharp
// Controllers/SystemController.cs
[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly IHostApplicationLifetime _lifetime;

    public SystemController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "Running",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("stop")]
    public IActionResult Stop()
    {
        _lifetime.StopApplication();
        return Ok(new { message = "Application stopping..." });
    }
}
```
