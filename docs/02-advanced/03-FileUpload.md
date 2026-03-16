# ASP.NET Core 檔案上傳與儲存管理

> 一句話摘要：涵蓋本地檔案儲存、驗證規則、分類管理、圖片處理到靜態存取的完整實作流程。

## 目錄

- [1. 設定與介面](#1-設定與介面)
- [2. 核心服務實作](#2-核心服務實作)
- [3. 檔案驗證](#3-檔案驗證)
- [4. 分類儲存](#4-分類儲存)
- [5. 圖片處理](#5-圖片處理)
- [6. Controller](#6-controller)
- [7. 靜態檔案存取](#7-靜態檔案存取)
- [8. 服務註冊](#8-服務註冊)
- [9. 測試方式](#9-測試方式)
- [10. 完整範例：使用者頭像上傳](#10-完整範例使用者頭像上傳)
- [最佳實踐](#最佳實踐)

---

## 1. 設定與介面

### appsettings.json

```json
{
  "FileStorage": {
    "UploadPath": "uploads",
    "MaxFileSizeMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".xlsx"]
  },
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

// Settings/FileStorageRules.cs
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

### 服務介面

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

---

## 2. 核心服務實作

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
        if (!IsValidFile(file))
            throw new InvalidOperationException("無效的檔案");

        var uploadPath = Path.Combine(_environment.ContentRootPath, _settings.UploadPath, folder);
        Directory.CreateDirectory(uploadPath);

        var extension = GetFileExtension(file);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("File saved: {FileName}", fileName);

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
            throw new FileNotFoundException("找不到檔案");

        return await File.ReadAllBytesAsync(fullPath);
    }

    public bool IsValidFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > _settings.MaxFileSizeBytes)
        {
            _logger.LogWarning("File too large: {Size} bytes", file.Length);
            return false;
        }

        var extension = GetFileExtension(file);
        if (!_settings.AllowedExtensions.Contains(extension.ToLower()))
        {
            _logger.LogWarning("Invalid file extension: {Extension}", extension);
            return false;
        }

        return true;
    }

    public string GetFileExtension(IFormFile file)
        => Path.GetExtension(file.FileName).ToLower();
}
```

---

## 3. 檔案驗證

驗證服務額外提供 Magic Number 檢查（防止偽造副檔名）。

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

        if (!ValidateFileType(file, allowedExtensions) || !ValidateFileSize(file, 5))
            return false;

        // 檢查 Magic Number，防止偽造副檔名
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            var isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            var isPng  = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
            var isGif  = buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38;

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
        return ValidateFileType(file, allowedExtensions) && ValidateFileSize(file, 10);
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

---

## 4. 分類儲存

依使用情境自動決定儲存路徑。

```csharp
// Services/Implementations/CategorizedFileStorageService.cs
namespace API.Services.Implementations;

public class CategorizedFileStorageService : LocalFileStorageService
{
    public CategorizedFileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<LocalFileStorageService> logger,
        IWebHostEnvironment environment)
        : base(settings, logger, environment) { }

    public Task<string> SaveAvatarAsync(IFormFile file, string userId)
        => SaveFileAsync(file, Path.Combine("avatars", userId));

    public Task<string> SaveProductImageAsync(IFormFile file, int productId)
        => SaveFileAsync(file, Path.Combine("products", productId.ToString()));

    public Task<string> SaveDocumentAsync(IFormFile file, string documentType)
        => SaveFileAsync(file, Path.Combine("documents", documentType));
}
```

### 依規則分類（Rule-Based）

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

    public Task<string> SaveImageAsync(IFormFile file)
    {
        ValidateFile(file, _rules.Images);
        return _fileStorage.SaveFileAsync(file, _rules.Images.Folder);
    }

    public Task<string> SaveDocumentAsync(IFormFile file)
    {
        ValidateFile(file, _rules.Documents);
        return _fileStorage.SaveFileAsync(file, _rules.Documents.Folder);
    }

    public Task<string> SaveVideoAsync(IFormFile file)
    {
        ValidateFile(file, _rules.Videos);
        return _fileStorage.SaveFileAsync(file, _rules.Videos.Folder);
    }

    private void ValidateFile(IFormFile file, FileTypeRule rule)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!rule.AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"不支援的檔案類型: {extension}");

        var maxBytes = rule.MaxSizeMB * 1024 * 1024;
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"檔案大小超過限制: {rule.MaxSizeMB}MB");
    }
}
```

---

## 5. 圖片處理

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
    Task<string> ResizeImageAsync(IFormFile file, int width, int height, string folder = "resized");
    Task<string> CreateThumbnailAsync(IFormFile file, string folder = "thumbnails");
}
```

```csharp
// Services/Implementations/ImageProcessingService.cs
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using API.Services.Interfaces;

namespace API.Services.Implementations;

public class ImageProcessingService : IImageProcessingService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(
        IWebHostEnvironment environment,
        ILogger<ImageProcessingService> logger)
    {
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

    public Task<string> CreateThumbnailAsync(IFormFile file, string folder = "thumbnails")
        => ResizeImageAsync(file, 200, 200, folder);
}
```

---

## 6. Controller

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
            ".png"  => "image/png",
            ".gif"  => "image/gif",
            ".pdf"  => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _       => "application/octet-stream"
        };
    }
}
```

---

## 7. 靜態檔案存取

```csharp
// Program.cs
var app = builder.Build();

app.UseStaticFiles(); // wwwroot

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.Run();
```

存取範例：

```
http://localhost:5000/uploads/avatars/user123/abc.jpg
```

---

## 8. 服務註冊

```csharp
// Program.cs
using API.Settings;
using API.Services.Interfaces;
using API.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// 設定
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<FileStorageRules>(
    builder.Configuration.GetSection("FileStorageRules"));

// 服務
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// 上傳大小限制
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

---

## 9. 測試方式

### Postman

1. 選擇 `POST` 方法
2. URL：`https://localhost:5001/api/files/upload`
3. Body → form-data
4. Key：`file`，Type：`File`
5. 選擇檔案送出

### curl

```bash
curl -X POST https://localhost:5001/api/files/upload \
  -F "file=@/path/to/your/file.jpg"
```

### HTML Form

```html
<form action="https://localhost:5001/api/files/upload" method="post" enctype="multipart/form-data">
    <input type="file" name="file" />
    <button type="submit">上傳</button>
</form>
```

---

## 10. 完整範例：使用者頭像上傳

### Entity

```csharp
// Entities/AppUser.cs
public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Controller Action

```csharp
// Controllers/UsersController.cs
[HttpPost("{id}/avatar")]
public async Task<IActionResult> UploadAvatar(string id, IFormFile file)
{
    try
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { error = "找不到使用者" });

        if (!_fileValidation.ValidateImage(file))
            return BadRequest(new { error = "無效的圖片檔案" });

        // 刪除舊頭像
        if (!string.IsNullOrEmpty(user.AvatarUrl))
            await _fileStorage.DeleteFileAsync(user.AvatarUrl);

        // 儲存新頭像與縮圖
        var avatarPath    = await _categorizedFileStorage.SaveAvatarAsync(file, id);
        var thumbnailPath = await _imageProcessing.CreateThumbnailAsync(file, $"avatars/{id}/thumbnails");

        await _userService.UpdateAvatarAsync(id, avatarPath);

        return Ok(new
        {
            avatarUrl    = avatarPath,
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

## 最佳實踐

**應該做的：**

- 驗證副檔名 + Magic Number，防止偽造檔案類型
- 限制檔案大小，避免伺服器資源耗盡
- 使用 GUID 作為儲存檔名，避免衝突
- 按使用情境分資料夾（avatars、products、documents）
- 更新時刪除舊檔案，避免孤立檔案積累
- 記錄上傳/刪除日誌，便於追蹤與除錯

**不應該做的：**

- 直接使用使用者提供的原始檔名
- 跳過檔案類型驗證
- 將所有檔案堆在同一個資料夾
- 不設定檔案大小上限
- 將敏感檔案放在可公開存取的路徑

### 推薦資料夾結構

```
uploads/
├── avatars/
│   └── {userId}/
│       ├── {guid}.jpg
│       └── thumbnails/
├── products/
│   └── {productId}/
├── documents/
│   ├── invoices/
│   └── reports/
└── temp/
```
