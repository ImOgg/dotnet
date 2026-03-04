# 通知系統 (Notifications) 完整指南

## 什麼是通知系統？

**通知系統**用於向使用者發送訊息，例如：
- 📧 Email 通知
- 📱 推播通知（Push Notifications）
- 💬 站內信息
- 📲 SMS 簡訊

---

## ASP.NET Core 通知系統架構

### 架構圖

```
Controller → INotificationService → [具體實作]
                                    ├─ EmailService
                                    ├─ PushNotificationService
                                    └─ SmsService
```

---

## 1. Email 通知系統

### 安裝套件

```bash
dotnet add package MailKit
dotnet add package MimeKit
```

### 設定 Email 配置

```json
// appsettings.json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "我的應用程式",
    "SenderEmail": "noreply@myapp.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### 建立 Email 設定類別

```csharp
// Settings/EmailSettings.cs
namespace API.Settings;

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### 建立 Email DTO

```csharp
// DTOs/EmailDto.cs
namespace API.DTOs;

public class EmailDto
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
}

public class EmailTemplateDto
{
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}
```

### 建立 Email Service

```csharp
// Services/IEmailService.cs
namespace API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(EmailDto emailDto);
    Task SendTemplateEmailAsync(string to, string templateName, Dictionary<string, string> parameters);
    Task SendBulkEmailAsync(List<string> recipients, string subject, string body);
}
```

```csharp
// Services/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using API.Settings;
using API.DTOs;

namespace API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var emailDto = new EmailDto
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        await SendEmailAsync(emailDto);
    }

    public async Task SendEmailAsync(EmailDto emailDto)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(emailDto.To));
            message.Subject = emailDto.Subject;

            // 加入 CC
            if (emailDto.Cc != null)
            {
                foreach (var cc in emailDto.Cc)
                {
                    message.Cc.Add(MailboxAddress.Parse(cc));
                }
            }

            // 加入 BCC
            if (emailDto.Bcc != null)
            {
                foreach (var bcc in emailDto.Bcc)
                {
                    message.Bcc.Add(MailboxAddress.Parse(bcc));
                }
            }

            var builder = new BodyBuilder();
            if (emailDto.IsHtml)
            {
                builder.HtmlBody = emailDto.Body;
            }
            else
            {
                builder.TextBody = emailDto.Body;
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To}: {Subject}", emailDto.To, emailDto.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", emailDto.To);
            throw;
        }
    }

    public async Task SendTemplateEmailAsync(
        string to,
        string templateName,
        Dictionary<string, string> parameters)
    {
        var template = GetEmailTemplate(templateName);
        var body = ReplaceParameters(template, parameters);

        await SendEmailAsync(to, GetSubject(templateName), body, isHtml: true);
    }

    public async Task SendBulkEmailAsync(
        List<string> recipients,
        string subject,
        string body)
    {
        var tasks = recipients.Select(email =>
            SendEmailAsync(email, subject, body));

        await Task.WhenAll(tasks);
    }

    private string GetEmailTemplate(string templateName)
    {
        // 從檔案或資料庫讀取模板
        return templateName switch
        {
            "Welcome" => @"
                <html>
                <body>
                    <h1>歡迎 {DisplayName}！</h1>
                    <p>感謝您註冊 {AppName}。</p>
                    <p>您的帳號: {Email}</p>
                    <a href='{ConfirmUrl}'>點此確認您的帳號</a>
                </body>
                </html>",

            "PasswordReset" => @"
                <html>
                <body>
                    <h1>重設密碼</h1>
                    <p>Hi {DisplayName},</p>
                    <p>點擊以下連結重設密碼:</p>
                    <a href='{ResetUrl}'>重設密碼</a>
                    <p>此連結將在 24 小時後失效。</p>
                </body>
                </html>",

            _ => "<p>{Message}</p>"
        };
    }

    private string GetSubject(string templateName)
    {
        return templateName switch
        {
            "Welcome" => "歡迎加入！",
            "PasswordReset" => "重設密碼",
            _ => "通知"
        };
    }

    private string ReplaceParameters(string template, Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            template = template.Replace($"{{{param.Key}}}", param.Value);
        }
        return template;
    }
}
```

### 註冊服務

```csharp
// Program.cs
using API.Settings;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

// 註冊 EmailSettings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// 註冊 EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();
```

### 在 Controller 中使用

```csharp
// Controllers/UsersController.cs
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public UsersController(
        ApplicationDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new AppUser
        {
            DisplayName = dto.DisplayName,
            Email = dto.Email
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 發送歡迎信
        var parameters = new Dictionary<string, string>
        {
            { "DisplayName", user.DisplayName },
            { "AppName", "我的應用" },
            { "Email", user.Email },
            { "ConfirmUrl", $"https://myapp.com/confirm?token={Guid.NewGuid()}" }
        };

        await _emailService.SendTemplateEmailAsync(
            user.Email,
            "Welcome",
            parameters);

        return Ok(new { message = "註冊成功，請查收確認信" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return BadRequest("找不到此帳號");

        var resetToken = Guid.NewGuid().ToString();

        // 發送重設密碼信
        var parameters = new Dictionary<string, string>
        {
            { "DisplayName", user.DisplayName },
            { "ResetUrl", $"https://myapp.com/reset-password?token={resetToken}" }
        };

        await _emailService.SendTemplateEmailAsync(
            user.Email,
            "PasswordReset",
            parameters);

        return Ok(new { message = "重設密碼連結已發送到您的信箱" });
    }
}
```

---

## 2. 站內通知系統

### 建立 Notification Entity

```csharp
// Entities/Notification.cs
namespace API.Entities;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Link { get; set; }

    // Navigation property
    public AppUser? User { get; set; }
}
```

### 更新 DbContext

```csharp
// Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 建立 Notification Service

```csharp
// Services/INotificationService.cs
namespace API.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message, string type = "Info", string? link = null);
    Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 50);
    Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteNotificationAsync(int notificationId);
}
```

```csharp
// Services/NotificationService.cs
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;

namespace API.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateNotificationAsync(
        string userId,
        string title,
        string message,
        string type = "Info",
        string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Notification created for user {UserId}: {Title}",
            userId, title);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 50)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Notification Controller

```csharp
// Controllers/NotificationsController.cs
using Microsoft.AspNetCore.Mvc;
using API.Services;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: api/notifications
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int limit = 50)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, limit);
        return Ok(notifications);
    }

    // GET: api/notifications/unread
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
        var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
        return Ok(notifications);
    }

    // PUT: api/notifications/5/read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    // PUT: api/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    // DELETE: api/notifications/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        await _notificationService.DeleteNotificationAsync(id);
        return NoContent();
    }
}
```

---

## 3. 統一通知服務

整合多種通知方式的統一介面：

```csharp
// Services/IUnifiedNotificationService.cs
namespace API.Services;

public interface IUnifiedNotificationService
{
    Task NotifyUserAsync(string userId, string title, string message, NotificationChannel channels);
}

[Flags]
public enum NotificationChannel
{
    None = 0,
    InApp = 1,      // 站內通知
    Email = 2,      // Email
    Sms = 4,        // 簡訊
    Push = 8        // 推播
}
```

```csharp
// Services/UnifiedNotificationService.cs
namespace API.Services;

public class UnifiedNotificationService : IUnifiedNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UnifiedNotificationService> _logger;
    private readonly ApplicationDbContext _context;

    public UnifiedNotificationService(
        INotificationService notificationService,
        IEmailService emailService,
        ApplicationDbContext context,
        ILogger<UnifiedNotificationService> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _context = context;
        _logger = logger;
    }

    public async Task NotifyUserAsync(
        string userId,
        string title,
        string message,
        NotificationChannel channels)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return;
        }

        var tasks = new List<Task>();

        // 站內通知
        if (channels.HasFlag(NotificationChannel.InApp))
        {
            tasks.Add(_notificationService.CreateNotificationAsync(
                userId, title, message, "Info"));
        }

        // Email 通知
        if (channels.HasFlag(NotificationChannel.Email))
        {
            tasks.Add(_emailService.SendEmailAsync(
                user.Email, title, message));
        }

        // 可擴展其他通知方式
        // if (channels.HasFlag(NotificationChannel.Sms))
        // {
        //     tasks.Add(_smsService.SendSmsAsync(user.PhoneNumber, message));
        // }

        await Task.WhenAll(tasks);
    }
}
```

### 使用範例

```csharp
// Controllers/OrdersController.cs
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var order = new Order { /* ... */ };
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    // 發送多種通知
    await _unifiedNotificationService.NotifyUserAsync(
        order.UserId,
        "訂單已建立",
        $"您的訂單 #{order.Id} 已成功建立",
        NotificationChannel.InApp | NotificationChannel.Email
    );

    return Ok(order);
}
```

---

## 4. 結合背景工作發送通知

```csharp
// Services/NotificationJobService.cs
public class NotificationJobService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationJobService> _logger;

    public NotificationJobService(
        IServiceProvider serviceProvider,
        ILogger<NotificationJobService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendBulkNotificationsAsync(
        List<string> userIds,
        string title,
        string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider
            .GetRequiredService<INotificationService>();

        foreach (var userId in userIds)
        {
            try
            {
                await notificationService.CreateNotificationAsync(
                    userId, title, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending notification to user {UserId}", userId);
            }
        }

        _logger.LogInformation(
            "Sent {Count} bulk notifications", userIds.Count);
    }
}
```

### 使用 Hangfire 排程發送

```csharp
// 批量發送通知
var userIds = await _context.Users
    .Where(u => u.IsActive)
    .Select(u => u.Id)
    .ToListAsync();

BackgroundJob.Enqueue<NotificationJobService>(
    x => x.SendBulkNotificationsAsync(
        userIds,
        "系統維護通知",
        "系統將於明天凌晨進行維護"));
```

---

## 註冊所有服務

```csharp
// Program.cs
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUnifiedNotificationService, UnifiedNotificationService>();
builder.Services.AddScoped<NotificationJobService>();
```

---

## 最佳實踐

### ✅ 應該做的

1. **使用背景工作發送通知**（避免阻塞請求）
2. **記錄通知發送狀態**
3. **處理發送失敗的情況**
4. **限制通知頻率**（避免垃圾通知）
5. **提供通知偏好設定**（讓使用者選擇接收方式）

### ❌ 不應該做的

1. ❌ 在主要請求中同步發送 Email
2. ❌ 沒有錯誤處理就批量發送
3. ❌ 將敏感資訊放在通知中
4. ❌ 沒有限制就無限制發送通知

---

## 總結

這個通知系統提供：
- ✅ Email 通知（使用 MailKit）
- ✅ 站內通知（儲存在資料庫）
- ✅ 統一通知介面
- ✅ 模板化 Email
- ✅ 背景工作整合
- ✅ 批量通知支援
