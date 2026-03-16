# ASP.NET Core 背景工作 (Background Jobs)

> 一句話摘要：涵蓋 BackgroundService、任務佇列到 Hangfire 的完整背景工作實作，包含概念對比、監控端點與最佳實踐。

## 目錄

- [什麼是背景工作](#什麼是背景工作)
- [三種方式比較](#三種方式比較)
- [1. BackgroundService](#1-backgroundservice)
- [2. 任務佇列](#2-任務佇列)
- [3. Hangfire](#3-hangfire)
- [完整範例：檔案匯入系統](#完整範例檔案匯入系統)
- [最佳實踐](#最佳實踐)

---

## 什麼是背景工作

背景工作在背景執行，不會阻塞主要的 HTTP 請求處理。

```
❌ 不好的做法：
User 發送請求 → 處理資料 → 發送 Email → 匯入 1000 筆資料 → 回應
                                    ↑
                            使用者要等很久！

✅ 好的做法：
User 發送請求 → 將工作加入佇列 → 立即回應 "已加入處理佇列"
                      ↓
                背景慢慢處理
```

### 適用場景

- 發送 Email / SMS
- 匯入 / 匯出大量資料
- 圖片 / 影片處理
- 產生報表
- 定期清理資料
- 呼叫第三方 API

---

## 三種方式比較

| 方式 | 適用場景 | 複雜度 | 持久化 | 分散式 |
|------|---------|--------|--------|--------|
| **IHostedService** | 定期任務、簡單背景工作 | 低 | 否 | 否 |
| **BackgroundService** | 持續運行的服務 | 低 | 否 | 否 |
| **Hangfire** | 複雜的任務管理 | 中 | 是 | 是 |

---

## 1. BackgroundService

`BackgroundService` 是 `IHostedService` 的基底類別，適合定期執行的簡單任務。

### 定期清理過期資料

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
        _logger.LogInformation("Data Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var expiredData = await dbContext.Users
                    .Where(u => u.CreatedAt < cutoffDate && u.IsDeleted)
                    .ToListAsync();

                dbContext.Users.RemoveRange(expiredData);
                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Cleaned up {Count} expired records.",
                    expiredData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred cleaning up data.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Cleanup Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
```

### 服務註冊

```csharp
// Program.cs
builder.Services.AddHostedService<DataCleanupService>();
```

---

## 2. 任務佇列

適合將耗時工作丟入佇列、立即回應使用者，由背景服務依序處理。

### 佇列介面與實作

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
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
```

### 佇列處理服務

```csharp
// Services/QueuedHostedService.cs
namespace API.Services;

public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing work item.");
            }
        }
    }
}
```

### 匯入服務（使用佇列）

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
            if (token.IsCancellationRequested) break;

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

        _logger.LogInformation("Import completed. {Count} users imported.", users.Count);

        File.Delete(filePath); // 刪除臨時檔案
    }
}
```

### Controller：接收上傳並加入佇列

```csharp
// Controllers/ImportController.cs
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

        var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", "temp");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}.csv";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        await _importService.QueueImportJobAsync(filePath);

        return Accepted(new
        {
            message = "檔案已接收，正在背景處理中",
            fileName = file.FileName
        });
    }
}
```

### Controller：監控應用程式狀態

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

### 服務註冊

```csharp
// Program.cs
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHostedService<DataCleanupService>();
```

### 測試

建立測試 CSV（`users.csv`）：

```csv
DisplayName,Email
張三,zhang@example.com
李四,li@example.com
```

上傳請求：

```http
POST https://localhost:5001/api/import/users
Content-Type: multipart/form-data

file: users.csv
```

回應：

```json
{
  "message": "檔案已接收，正在背景處理中",
  "fileName": "users.csv"
}
```

---

## 3. Hangfire

適合需要持久化、排程、重試機制與 Dashboard 監控的複雜場景。

### 安裝

```bash
dotnet add package Hangfire.Core
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.MySql
```

### 設定

```csharp
// Program.cs
using Hangfire;
using Hangfire.MySql;

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlStorageOptions { TablesPrefix = "Hangfire_" }
    )));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // 生產環境應加入身份驗證
    // Authorization = new[] { new MyAuthorizationFilter() }
});

app.Run();
```

### 定義工作

```csharp
// Services/JobService.cs
public interface IJobService
{
    Task SendWelcomeEmailAsync(string userId);
    Task ProcessImportAsync(string filePath);
    Task GenerateReportAsync(int reportId);
}

public class JobService : IJobService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<JobService> _logger;

    public JobService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<JobService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return;
        }

        await _emailService.SendEmailAsync(
            user.Email,
            "歡迎加入",
            $"Hi {user.DisplayName}, 歡迎加入我們的平台！");

        _logger.LogInformation("Welcome email sent to {Email}", user.Email);
    }

    public async Task ProcessImportAsync(string filePath)
    {
        _logger.LogInformation("Starting import from {FilePath}", filePath);

        var lines = await File.ReadAllLinesAsync(filePath);
        var successCount = 0;
        var errorCount = 0;

        foreach (var line in lines)
        {
            try
            {
                var data = ParseLine(line);
                _context.Users.Add(data);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing line: {Line}", line);
                errorCount++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation(
            "Import completed. Success: {Success}, Errors: {Errors}",
            successCount, errorCount);
    }

    public async Task GenerateReportAsync(int reportId)
    {
        _logger.LogInformation("Generating report {ReportId}", reportId);
        await Task.Delay(5000); // 模擬耗時操作
        _logger.LogInformation("Report {ReportId} generated", reportId);
    }

    private AppUser ParseLine(string line)
    {
        var parts = line.Split(',');
        return new AppUser { DisplayName = parts[0], Email = parts[1] };
    }
}
```

### 四種執行方式

#### 立即執行（Fire-and-forget）

```csharp
// 建立使用者後，在背景發送歡迎信
BackgroundJob.Enqueue<IJobService>(x => x.SendWelcomeEmailAsync(user.Id));
```

#### 延遲執行（Delayed）

```csharp
// 1 小時後發送提醒
BackgroundJob.Schedule<IJobService>(
    x => x.SendReminderEmailAsync(userId),
    TimeSpan.FromHours(1));
```

#### 重複執行（Recurring）

```csharp
// 每天凌晨 2 點清理資料
RecurringJob.AddOrUpdate<IJobService>(
    "cleanup-expired-data",
    x => x.CleanupExpiredDataAsync(),
    "0 2 * * *");

// 每 5 分鐘檢查通知
RecurringJob.AddOrUpdate<IJobService>(
    "check-notifications",
    x => x.CheckNotificationsAsync(),
    "*/5 * * * *");
```

#### 連續執行（Continuations）

```csharp
// 匯入完成後發送通知
var importJobId = BackgroundJob.Enqueue<IJobService>(
    x => x.ProcessImportAsync(filePath));

BackgroundJob.ContinueJobWith<IJobService>(
    importJobId,
    x => x.SendImportCompletedEmailAsync(userId));
```

### Cron 表達式參考

```
格式：分 時 日 月 星期

*/5 * * * *     每 5 分鐘
0 * * * *       每小時
0 0 * * *       每天凌晨 12 點
0 2 * * *       每天凌晨 2 點
0 0 * * 1       每週一凌晨 12 點
0 0 1 * *       每月 1 號凌晨 12 點
0 9 * * 1-5     週一到週五早上 9 點
```

---

## 完整範例：檔案匯入系統（Hangfire 版）

### DTO

```csharp
// DTOs/ImportJobDto.cs
public class ImportJobDto
{
    public string JobId { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}
```

### 服務

```csharp
// Services/UserImportService.cs
public interface IUserImportService
{
    Task<string> QueueImportAsync(IFormFile file, string userId);
    Task ProcessImportAsync(string filePath, string userId);
}

public class UserImportService : IUserImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserImportService> _logger;

    public UserImportService(
        ApplicationDbContext context,
        ILogger<UserImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> QueueImportAsync(IFormFile file, string userId)
    {
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var jobId = BackgroundJob.Enqueue<IUserImportService>(
            x => x.ProcessImportAsync(filePath, userId));

        _logger.LogInformation("Import job {JobId} queued for user {UserId}", jobId, userId);

        return jobId;
    }

    public async Task ProcessImportAsync(string filePath, string userId)
    {
        _logger.LogInformation("Processing import from {FilePath}", filePath);

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var users = new List<AppUser>();

            foreach (var line in lines.Skip(1))
            {
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

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Import completed. {Count} users imported.", users.Count);

            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing import");
            throw; // Hangfire 會自動重試
        }
    }
}
```

### Controller

```csharp
// Controllers/ImportController.cs
[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IUserImportService _importService;

    public ImportController(IUserImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("users")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請選擇 CSV 檔案");

        if (!file.FileName.EndsWith(".csv"))
            return BadRequest("只接受 CSV 檔案");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var jobId = await _importService.QueueImportAsync(file, userId);

        return Accepted(new ImportJobDto
        {
            JobId = jobId,
            Status = "Queued",
            Message = "檔案已加入處理佇列，請稍後查看結果"
        });
    }

    [HttpGet("status/{jobId}")]
    public IActionResult GetJobStatus(string jobId)
    {
        var jobDetails = JobStorage.Current
            .GetConnection()
            .GetJobData(jobId);

        if (jobDetails == null)
            return NotFound("找不到該工作");

        return Ok(new
        {
            jobId = jobId,
            state = jobDetails.State,
            createdAt = jobDetails.CreatedAt
        });
    }
}
```

---

## 最佳實踐

**應該做的：**

- 使用 `ILogger` 記錄工作開始、完成與錯誤
- 使用 `CancellationToken` 支援優雅關閉
- 在 BackgroundService 中透過 `IServiceProvider.CreateScope()` 存取 Scoped 服務（如 DbContext）
- Hangfire 中 `throw` 讓例外傳出，觸發自動重試
- 只傳遞 ID 給背景工作，不傳大型物件

**不應該做的：**

- 在 `BackgroundService` 建構子中直接注入 `DbContext`（Scoped 服務）
- 在 Hangfire 工作中傳遞大型物件（應傳 ID 再從 DB 查詢）
- 生產環境不保護 Hangfire Dashboard（需加身份驗證）

**方案選擇：**

| 場景 | 建議 |
|------|------|
| 定期清理、簡單排程 | `BackgroundService` |
| 即時佇列、解耦請求 | `BackgroundTaskQueue` + `QueuedHostedService` |
| 需持久化、重試、監控 | Hangfire |

---

## 參考資源

- [Hangfire 官方文件](https://docs.hangfire.io/)
- [IHostedService 官方文件](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [Cron 表達式產生器](https://crontab.guru/)
