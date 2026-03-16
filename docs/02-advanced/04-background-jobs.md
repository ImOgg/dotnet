# 背景工作 (Background Jobs) 完整指南

## 什麼是背景工作？

**背景工作（Background Jobs）** 是在背景執行的任務，不會阻塞主要的 HTTP 請求處理。

### 常見使用場景

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

1. **📧 發送 Email/SMS**
2. **📊 匯入/匯出大量資料**
3. **🖼️ 圖片/影片處理**
4. **📝 產生報表**
5. **🔄 定期清理資料**
6. **📡 呼叫第三方 API**

---

## ASP.NET Core 背景工作的三種方式

### 方式對照表

| 方式 | 適用場景 | 複雜度 | 持久化 | 分散式 |
|------|---------|--------|--------|--------|
| **IHostedService** | 定期任務、簡單背景工作 | 低 | ❌ | ❌ |
| **BackgroundService** | 持續運行的服務 | 低 | ❌ | ❌ |
| **Hangfire** | 複雜的任務管理 | 中 | ✅ | ✅ |

---

## 1. IHostedService / BackgroundService

### 基本概念

**IHostedService** 是 ASP.NET Core 內建的背景服務介面。

**BackgroundService** 是更簡單的基底類別（繼承自 IHostedService）。

### 範例：定期清理過期資料

#### 步驟 1：建立 BackgroundService

```csharp
// Services/DataCleanupService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        // 每小時執行一次
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Data Cleanup Service is working.");

                // 使用 Scope 來取得 DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                    // 清理 30 天前的資料
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred cleaning up data.");
            }

            // 等待 1 小時
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

#### 步驟 2：註冊服務

```csharp
// Program.cs
builder.Services.AddHostedService<DataCleanupService>();
```

### 範例：匯入資料的背景工作

```csharp
// Services/ImportService.cs
public interface IImportService
{
    Task QueueImportJob(string filePath);
}

public class ImportService : IImportService
{
    private readonly IBackgroundTaskQueue _taskQueue;

    public ImportService(IBackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public async Task QueueImportJob(string filePath)
    {
        await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
        {
            // 實際的匯入邏輯
            await ProcessImportAsync(filePath, token);
        });
    }

    private async Task ProcessImportAsync(string filePath, CancellationToken token)
    {
        // 讀取檔案
        var lines = await File.ReadAllLinesAsync(filePath, token);

        foreach (var line in lines)
        {
            if (token.IsCancellationRequested)
                break;

            // 處理每一行資料
            await ProcessLineAsync(line);
        }
    }

    private async Task ProcessLineAsync(string line)
    {
        // 解析並儲存資料
        await Task.Delay(100); // 模擬處理時間
    }
}
```

### 任務佇列實作

```csharp
// Services/BackgroundTaskQueue.cs
using System.Threading.Channels;

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

### 佇列處理服務

```csharp
// Services/QueuedHostedService.cs
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

### 註冊所有服務

```csharp
// Program.cs
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddHostedService<QueuedHostedService>();
```

### 在 Controller 中使用

```csharp
// Controllers/ImportController.cs
[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        IImportService importService,
        ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("請選擇檔案");

        // 儲存檔案
        var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 加入背景處理佇列
        await _importService.QueueImportJob(filePath);

        return Accepted(new
        {
            message = "檔案已接收，正在背景處理中",
            fileName = file.FileName
        });
    }
}
```

---

## 2. Hangfire（推薦用於複雜場景）

### 為什麼選擇 Hangfire？

✅ **持久化**：工作儲存在資料庫，重啟不會遺失
✅ **排程**：支援 Cron 表達式
✅ **重試機制**：自動重試失敗的工作
✅ **Dashboard**：視覺化監控介面
✅ **分散式**：支援多台伺服器

### 安裝 Hangfire

```bash
dotnet add package Hangfire.Core
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.MySql
```

### 設定 Hangfire

```csharp
// Program.cs
using Hangfire;
using Hangfire.MySql;

// 設定 Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlStorageOptions
        {
            TablesPrefix = "Hangfire_"
        }
    )));

// 加入 Hangfire Server
builder.Services.AddHangfireServer();

var app = builder.Build();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // 生產環境應該加入身份驗證
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
            $"Hi {user.DisplayName}, 歡迎加入我們的平台！"
        );

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
                // 解析並儲存資料
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

        // 模擬耗時的報表產生
        await Task.Delay(5000);

        _logger.LogInformation("Report {ReportId} generated", reportId);
    }

    private AppUser ParseLine(string line)
    {
        var parts = line.Split(',');
        return new AppUser
        {
            DisplayName = parts[0],
            Email = parts[1]
        };
    }
}
```

### 使用 Hangfire

#### 1. 立即執行（Fire-and-forget）

```csharp
// Controllers/UsersController.cs
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserDto dto)
{
    var user = new AppUser
    {
        DisplayName = dto.DisplayName,
        Email = dto.Email
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // 在背景發送歡迎信
    BackgroundJob.Enqueue<IJobService>(
        x => x.SendWelcomeEmailAsync(user.Id));

    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}
```

#### 2. 延遲執行（Delayed）

```csharp
// 1 小時後發送提醒
BackgroundJob.Schedule<IJobService>(
    x => x.SendReminderEmailAsync(userId),
    TimeSpan.FromHours(1));
```

#### 3. 重複執行（Recurring）

```csharp
// Program.cs - 在應用程式啟動時設定
app.UseHangfireDashboard("/hangfire");

// 每天凌晨 2 點清理資料
RecurringJob.AddOrUpdate<IJobService>(
    "cleanup-expired-data",
    x => x.CleanupExpiredDataAsync(),
    "0 2 * * *");  // Cron 表達式

// 每 5 分鐘檢查一次
RecurringJob.AddOrUpdate<IJobService>(
    "check-notifications",
    x => x.CheckNotificationsAsync(),
    "*/5 * * * *");
```

#### 4. 連續執行（Continuations）

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

範例：
*/5 * * * *     每 5 分鐘
0 * * * *       每小時
0 0 * * *       每天凌晨 12 點
0 2 * * *       每天凌晨 2 點
0 0 * * 1       每週一凌晨 12 點
0 0 1 * *       每月 1 號凌晨 12 點
0 9 * * 1-5     週一到週五早上 9 點
```

---

## 完整範例：檔案匯入系統

### 1. 定義 DTO

```csharp
// DTOs/ImportDto.cs
public class ImportJobDto
{
    public string JobId { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}
```

### 2. 建立服務

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
        // 儲存檔案
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 加入 Hangfire 佇列
        var jobId = BackgroundJob.Enqueue<IUserImportService>(
            x => x.ProcessImportAsync(filePath, userId));

        _logger.LogInformation(
            "Import job {JobId} queued for user {UserId}",
            jobId, userId);

        return jobId;
    }

    public async Task ProcessImportAsync(string filePath, string userId)
    {
        _logger.LogInformation("Processing import from {FilePath}", filePath);

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var users = new List<AppUser>();

            foreach (var line in lines.Skip(1)) // 跳過標題行
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

            _logger.LogInformation(
                "Import completed. {Count} users imported.",
                users.Count);

            // 刪除檔案
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing import");
            throw;
        }
    }
}
```

### 3. Controller

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

### ✅ 應該做的

1. **使用 ILogger 記錄**
   ```csharp
   _logger.LogInformation("Job started: {JobId}", jobId);
   ```

2. **處理例外**
   ```csharp
   try
   {
       await ProcessDataAsync();
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Job failed");
       throw; // Hangfire 會自動重試
   }
   ```

3. **使用 CancellationToken**
   ```csharp
   public async Task ProcessAsync(CancellationToken cancellationToken)
   {
       if (cancellationToken.IsCancellationRequested)
           return;
   }
   ```

4. **限制重試次數**
   ```csharp
   [AutomaticRetry(Attempts = 3)]
   public async Task RiskyJobAsync()
   {
       // ...
   }
   ```

### ❌ 不應該做的

1. ❌ 在背景工作中使用 Scoped 服務（DbContext）
   ```csharp
   // 錯誤
   public class MyJob
   {
       private readonly ApplicationDbContext _context; // ❌

       public MyJob(ApplicationDbContext context)
       {
           _context = context;
       }
   }

   // 正確
   public class MyJob
   {
       private readonly IServiceProvider _serviceProvider; // ✅

       public async Task ExecuteAsync()
       {
           using var scope = _serviceProvider.CreateScope();
           var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
       }
   }
   ```

2. ❌ 傳遞大型物件
   ```csharp
   // 錯誤
   BackgroundJob.Enqueue(() => Process(largeObject)); // ❌

   // 正確
   BackgroundJob.Enqueue(() => Process(id)); // ✅ 只傳 ID
   ```

---

## 方案比較

### 簡單場景：使用 BackgroundService
```csharp
適合：
- 定期清理
- 簡單的背景任務
- 不需要持久化
```

### 複雜場景：使用 Hangfire
```csharp
適合：
- 匯入/匯出
- 需要監控
- 需要重試
- 多台伺服器
```

---

## 參考資源

- [Hangfire 官方文件](https://docs.hangfire.io/)
- [IHostedService 官方文件](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [Cron 表達式產生器](https://crontab.guru/)
