# 檔案儲存與管理完整指南

## 什麼是檔案儲存?

在 ASP.NET Core 中,檔案儲存主要用於:
- 📷 圖片上傳 (個人頭像、商品圖片)
- 📄 文件上傳 (PDF、Word、Excel)
- 🎵 媒體檔案 (音訊、影片)
- 📊 CSV/Excel 匯入

---

## 1. 本地檔案儲存

### 基本概念

將檔案儲存在伺服器的檔案系統中。

### 設定

```json
// appsettings.json
{
  "FileStorage": {
    "UploadPath": "uploads",
    "MaxFileSizeMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".xlsx"]
  }
}
```

### 設定類別

```csharp
// Settings/FileStorageSettings.cs
namespace API.Settings;

public class FileStorageSettings
{
    public string UploadPath { get; set; } = "uploads";
    public int MaxFileSizeMB { get; set; } = 10;
    public List<string> AllowedExtensions { get; set; } = new();

    public long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
}
```

### 檔案上傳服務

```csharp
// Services/Interfaces/IFileStorageService.cs
namespace API.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder = "general");
    Task<bool> DeleteFileAsync(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
    bool IsValidFile(IFormFile file);
    string GetFileExtension(IFormFile file);
}
```

```csharp
// Services/Implementations/LocalFileStorageService.cs
using Microsoft.Extensions.Options;
using API.Settings;
using API.Services.Interfaces;

namespace API.Services.Implementations;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorageService> logger,
        IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder = "general")
    {
        // 驗證檔案
        if (!IsValidFile(file))
        {
            throw new InvalidOperationException("無效的檔案");
        }

        // 建立資料夾
        var uploadPath = Path.Combine(_environment.ContentRootPath, _settings.UploadPath, folder);
        Directory.CreateDirectory(uploadPath);

        // 產生唯一檔名
        var extension = GetFileExtension(file);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        // 儲存檔案
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("File saved: {FileName}", fileName);

        // 回傳相對路徑
        return Path.Combine(folder, fileName).Replace("\\", "/");
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, _settings.UploadPath, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, _settings.UploadPath, filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("找不到檔案");
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public bool IsValidFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        // 檢查檔案大小
        if (file.Length > _settings.MaxFileSizeBytes)
        {
            _logger.LogWarning("File too large: {Size} bytes", file.Length);
            return false;
        }

        // 檢查副檔名
        var extension = GetFileExtension(file);
        if (!_settings.AllowedExtensions.Contains(extension.ToLower()))
        {
            _logger.LogWarning("Invalid file extension: {Extension}", extension);
            return false;
        }

        return true;
    }

    public string GetFileExtension(IFormFile file)
    {
        return Path.GetExtension(file.FileName).ToLower();
    }
}
```

### Controller

```csharp
// Controllers/FilesController.cs
using Microsoft.AspNetCore.Mvc;
using API.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorage,
        ILogger<FilesController> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    // POST: api/files/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folder = "general")
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "請選擇檔案" });

            if (!_fileStorage.IsValidFile(file))
                return BadRequest(new { error = "無效的檔案類型或檔案過大" });

            var filePath = await _fileStorage.SaveFileAsync(file, folder);

            return Ok(new
            {
                fileName = file.FileName,
                filePath = filePath,
                fileSize = file.Length,
                uploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "上傳失敗" });
        }
    }

    // POST: api/files/upload-multiple
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files, [FromQuery] string folder = "general")
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "請選擇至少一個檔案" });

        var results = new List<object>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            try
            {
                if (!_fileStorage.IsValidFile(file))
                {
                    errors.Add($"{file.FileName}: 無效的檔案");
                    continue;
                }

                var filePath = await _fileStorage.SaveFileAsync(file, folder);
                results.Add(new
                {
                    fileName = file.FileName,
                    filePath = filePath,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                errors.Add($"{file.FileName}: 上傳失敗");
            }
        }

        return Ok(new
        {
            uploadedFiles = results,
            errors = errors,
            totalUploaded = results.Count,
            totalErrors = errors.Count
        });
    }

    // GET: api/files/download/{folder}/{filename}
    [HttpGet("download/{folder}/{filename}")]
    public async Task<IActionResult> DownloadFile(string folder, string filename)
    {
        try
        {
            var filePath = Path.Combine(folder, filename);
            var fileBytes = await _fileStorage.GetFileAsync(filePath);
            var contentType = GetContentType(filename);

            return File(fileBytes, contentType, filename);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "找不到檔案" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, new { error = "下載失敗" });
        }
    }

    // DELETE: api/files/{folder}/{filename}
    [HttpDelete("{folder}/{filename}")]
    public async Task<IActionResult> DeleteFile(string folder, string filename)
    {
        var filePath = Path.Combine(folder, filename);
        var result = await _fileStorage.DeleteFileAsync(filePath);

        if (result)
            return NoContent();

        return NotFound(new { error = "找不到檔案" });
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
```

### 註冊服務

```csharp
// Program.cs
using API.Settings;
using API.Services.Interfaces;
using API.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// 註冊檔案儲存設定
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));

// 註冊檔案儲存服務
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();
```

---

## 2. 檔案儲存規則 (Storage Rules)

### 建立驗證規則

```csharp
// Services/FileValidationService.cs
namespace API.Services;

public interface IFileValidationService
{
    bool ValidateImage(IFormFile file);
    bool ValidateDocument(IFormFile file);
    bool ValidateFileSize(IFormFile file, int maxSizeMB);
    bool ValidateFileType(IFormFile file, List<string> allowedExtensions);
}

public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;

    public FileValidationService(ILogger<FileValidationService> logger)
    {
        _logger = logger;
    }

    public bool ValidateImage(IFormFile file)
    {
        var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
        var maxSizeMB = 5;

        if (!ValidateFileType(file, allowedExtensions))
        {
            _logger.LogWarning("Invalid image type: {FileName}", file.FileName);
            return false;
        }

        if (!ValidateFileSize(file, maxSizeMB))
        {
            _logger.LogWarning("Image too large: {FileName}, Size: {Size}", file.FileName, file.Length);
            return false;
        }

        // 驗證是否為真實的圖片檔案 (檢查 Magic Number)
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            // JPEG: FF D8 FF
            // PNG: 89 50 4E 47
            // GIF: 47 49 46 38
            var isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            var isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
            var isGif = buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38;

            if (!isJpeg && !isPng && !isGif)
            {
                _logger.LogWarning("File is not a valid image: {FileName}", file.FileName);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image: {FileName}", file.FileName);
            return false;
        }

        return true;
    }

    public bool ValidateDocument(IFormFile file)
    {
        var allowedExtensions = new List<string> { ".pdf", ".docx", ".xlsx", ".txt" };
        var maxSizeMB = 10;

        return ValidateFileType(file, allowedExtensions) && ValidateFileSize(file, maxSizeMB);
    }

    public bool ValidateFileSize(IFormFile file, int maxSizeMB)
    {
        var maxBytes = maxSizeMB * 1024 * 1024;
        return file.Length > 0 && file.Length <= maxBytes;
    }

    public bool ValidateFileType(IFormFile file, List<string> allowedExtensions)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(extension);
    }
}
```

### 按照資料夾分類儲存

```csharp
// Services/Implementations/CategorizedFileStorageService.cs
namespace API.Services.Implementations;

public class CategorizedFileStorageService : LocalFileStorageService
{
    public CategorizedFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorageService> logger,
        IWebHostEnvironment environment)
        : base(settings, logger, environment)
    {
    }

    public async Task<string> SaveAvatarAsync(IFormFile file, string userId)
    {
        var folder = Path.Combine("avatars", userId);
        return await SaveFileAsync(file, folder);
    }

    public async Task<string> SaveProductImageAsync(IFormFile file, int productId)
    {
        var folder = Path.Combine("products", productId.ToString());
        return await SaveFileAsync(file, folder);
    }

    public async Task<string> SaveDocumentAsync(IFormFile file, string documentType)
    {
        var folder = Path.Combine("documents", documentType);
        return await SaveFileAsync(file, folder);
    }
}
```

---

## 3. 圖片處理

### 安裝套件

```bash
dotnet add package SixLabors.ImageSharp
```

### 圖片處理服務

```csharp
// Services/IImageProcessingService.cs
namespace API.Services;

public interface IImageProcessingService
{
    Task<string> ResizeImageAsync(IFormFile file, int width, int height, string folder = "thumbnails");
    Task<string> CreateThumbnailAsync(IFormFile file, string folder = "thumbnails");
}
```

```csharp
// Services/ImageProcessingService.cs
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using API.Services.Interfaces;

namespace API.Services.Implementations;

public class ImageProcessingService : IImageProcessingService
{
    private readonly IFileStorageService _fileStorage;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(
        IFileStorageService fileStorage,
        IWebHostEnvironment environment,
        ILogger<ImageProcessingService> logger)
    {
        _fileStorage = fileStorage;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> ResizeImageAsync(IFormFile file, int width, int height, string folder = "resized")
    {
        using var image = await Image.LoadAsync(file.OpenReadStream());

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Max
        }));

        var fileName = $"{Guid.NewGuid()}.jpg";
        var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);
        await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });

        _logger.LogInformation("Image resized: {FileName}, Size: {Width}x{Height}", fileName, width, height);

        return Path.Combine(folder, fileName).Replace("\\", "/");
    }

    public async Task<string> CreateThumbnailAsync(IFormFile file, string folder = "thumbnails")
    {
        return await ResizeImageAsync(file, 200, 200, folder);
    }
}
```

---

## 4. 完整範例: 使用者頭像上傳

### Entity

```csharp
// Entities/AppUser.cs (新增欄位)
public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }  // 新增
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Controller

```csharp
// Controllers/UsersController.cs (新增方法)
[HttpPost("{id}/avatar")]
public async Task<IActionResult> UploadAvatar(string id, IFormFile file)
{
    try
    {
        // 驗證使用者
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { error = "找不到使用者" });

        // 驗證圖片
        if (!_fileValidation.ValidateImage(file))
            return BadRequest(new { error = "無效的圖片檔案" });

        // 刪除舊頭像
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            await _fileStorage.DeleteFileAsync(user.AvatarUrl);
        }

        // 儲存新頭像
        var avatarPath = await _categorizedFileStorage.SaveAvatarAsync(file, id);

        // 建立縮圖
        var thumbnailPath = await _imageProcessing.CreateThumbnailAsync(file, $"avatars/{id}/thumbnails");

        // 更新使用者資料
        await _userService.UpdateAvatarAsync(id, avatarPath);

        return Ok(new
        {
            avatarUrl = avatarPath,
            thumbnailUrl = thumbnailPath
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error uploading avatar for user {UserId}", id);
        return StatusCode(500, new { error = "上傳失敗" });
    }
}
```

---

## 5. 提供靜態檔案存取

### 設定靜態檔案中介軟體

```csharp
// Program.cs
var app = builder.Build();

// 設定靜態檔案
app.UseStaticFiles(); // wwwroot

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.Run();
```

現在可以透過 URL 存取檔案:
```
http://localhost:5000/uploads/avatars/user123/abc.jpg
```

---

## 6. 檔案分類規則範例

### 建立規則設定

```csharp
// Settings/FileStorageRules.cs
namespace API.Settings;

public class FileStorageRules
{
    public FileTypeRule Images { get; set; } = new();
    public FileTypeRule Documents { get; set; } = new();
    public FileTypeRule Videos { get; set; } = new();
}

public class FileTypeRule
{
    public List<string> AllowedExtensions { get; set; } = new();
    public int MaxSizeMB { get; set; }
    public string Folder { get; set; } = string.Empty;
}
```

### 設定檔

```json
// appsettings.json
{
  "FileStorageRules": {
    "Images": {
      "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"],
      "MaxSizeMB": 5,
      "Folder": "images"
    },
    "Documents": {
      "AllowedExtensions": [".pdf", ".docx", ".xlsx"],
      "MaxSizeMB": 10,
      "Folder": "documents"
    },
    "Videos": {
      "AllowedExtensions": [".mp4", ".avi", ".mov"],
      "MaxSizeMB": 100,
      "Folder": "videos"
    }
  }
}
```

### 使用規則

```csharp
// Services/RuleBasedFileStorageService.cs
using Microsoft.Extensions.Options;
using API.Settings;

namespace API.Services;

public class RuleBasedFileStorageService
{
    private readonly FileStorageRules _rules;
    private readonly IFileStorageService _fileStorage;

    public RuleBasedFileStorageService(
        IOptions<FileStorageRules> rules,
        IFileStorageService fileStorage)
    {
        _rules = rules.Value;
        _fileStorage = fileStorage;
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        ValidateFile(file, _rules.Images);
        return await _fileStorage.SaveFileAsync(file, _rules.Images.Folder);
    }

    public async Task<string> SaveDocumentAsync(IFormFile file)
    {
        ValidateFile(file, _rules.Documents);
        return await _fileStorage.SaveFileAsync(file, _rules.Documents.Folder);
    }

    public async Task<string> SaveVideoAsync(IFormFile file)
    {
        ValidateFile(file, _rules.Videos);
        return await _fileStorage.SaveFileAsync(file, _rules.Videos.Folder);
    }

    private void ValidateFile(IFormFile file, FileTypeRule rule)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!rule.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"不支援的檔案類型: {extension}");
        }

        var maxBytes = rule.MaxSizeMB * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new InvalidOperationException($"檔案大小超過限制: {rule.MaxSizeMB}MB");
        }
    }
}
```

---

## 最佳實踐

### ✅ 應該做的

1. **驗證檔案類型** - 檢查副檔名和 Magic Number
2. **限制檔案大小** - 避免伺服器被塞爆
3. **使用唯一檔名** - 避免檔名衝突 (使用 GUID)
4. **分類儲存** - 按照使用者、商品等分資料夾
5. **刪除舊檔案** - 更新時記得刪除舊檔
6. **記錄日誌** - 追蹤上傳/刪除操作

### ❌ 不應該做的

1. ❌ 直接使用使用者提供的檔名
2. ❌ 不驗證檔案類型
3. ❌ 將所有檔案存在同一個資料夾
4. ❌ 不限制檔案大小
5. ❌ 將敏感檔案放在可公開存取的位置

---

## 總結

這份指南涵蓋了:
- ✅ 本地檔案儲存實作
- ✅ 檔案驗證規則
- ✅ 分類儲存機制
- ✅ 圖片處理與縮圖
- ✅ 靜態檔案存取設定
- ✅ 可分離的儲存規則設定

### 推薦的檔案結構

```
uploads/
├── avatars/           # 使用者頭像
│   └── {userId}/
│       ├── avatar.jpg
│       └── thumbnails/
├── products/          # 商品圖片
│   └── {productId}/
├── documents/         # 文件
│   ├── invoices/
│   └── reports/
└── temp/             # 暫存檔案
```
