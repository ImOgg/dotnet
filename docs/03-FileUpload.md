# ASP.NET Web API 檔案上傳範例

## 1. 基本檔案上傳

```csharp
// Controllers/FilesController.cs
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IWebHostEnvironment environment,
        ILogger<FilesController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    // POST: api/files/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "請選擇檔案" });

        // 建立 uploads 資料夾
        var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        // 產生唯一檔名
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        // 儲存檔案
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("File uploaded: {FileName}", fileName);

        return Ok(new
        {
            fileName = fileName,
            originalFileName = file.FileName,
            fileSize = file.Length,
            contentType = file.ContentType,
            uploadPath = $"/uploads/{fileName}"
        });
    }

    // POST: api/files/upload-multiple
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "請選擇至少一個檔案" });

        var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var uploadedFiles = new List<object>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                uploadedFiles.Add(new
                {
                    fileName = fileName,
                    originalFileName = file.FileName,
                    fileSize = file.Length
                });
            }
        }

        return Ok(new
        {
            totalFiles = uploadedFiles.Count,
            files = uploadedFiles
        });
    }

    // GET: api/files/download/{fileName}
    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, "uploads", fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "找不到檔案" });

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var contentType = GetContentType(fileName);

        return File(fileBytes, contentType, fileName);
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

## 2. 使用檔案驗證

```csharp
// Services/FileValidationService.cs
namespace API.Services;

public interface IFileValidationService
{
    bool ValidateFile(IFormFile file);
    bool ValidateImage(IFormFile file);
}

public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

    private readonly List<string> _allowedImageExtensions = new()
    {
        ".jpg", ".jpeg", ".png", ".gif"
    };

    private readonly List<string> _allowedDocumentExtensions = new()
    {
        ".pdf", ".docx", ".xlsx", ".txt"
    };

    public FileValidationService(ILogger<FileValidationService> logger)
    {
        _logger = logger;
    }

    public bool ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File is null or empty");
            return false;
        }

        if (file.Length > _maxFileSize)
        {
            _logger.LogWarning("File size exceeds limit: {Size}", file.Length);
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLower();
        var allAllowedExtensions = _allowedImageExtensions
            .Concat(_allowedDocumentExtensions)
            .ToList();

        if (!allAllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Invalid file extension: {Extension}", extension);
            return false;
        }

        return true;
    }

    public bool ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLower();
        return _allowedImageExtensions.Contains(extension);
    }
}
```

```csharp
// 更新 Controller 使用驗證
[HttpPost("upload-validated")]
public async Task<IActionResult> UploadValidatedFile(IFormFile file)
{
    if (!_validationService.ValidateFile(file))
        return BadRequest(new { error = "無效的檔案" });

    // ... 上傳邏輯
}
```

## 3. 測試檔案上傳

### 使用 Postman
1. 選擇 POST 方法
2. URL: `https://localhost:5001/api/files/upload`
3. Body → form-data
4. Key: `file`, Type: File
5. 選擇檔案並送出

### 使用 curl
```bash
curl -X POST https://localhost:5001/api/files/upload \
  -F "file=@C:\path\to\your\file.jpg"
```

### 使用 HTML Form
```html
<form action="https://localhost:5001/api/files/upload" method="post" enctype="multipart/form-data">
    <input type="file" name="file" />
    <button type="submit">上傳</button>
</form>
```

## 4. 註冊服務

```csharp
// Program.cs
builder.Services.AddScoped<IFileValidationService, FileValidationService>();

// 設定檔案上傳大小限制
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

## 5. 提供靜態檔案存取

```csharp
// Program.cs
var app = builder.Build();

// 允許存取 uploads 資料夾
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.Run();
```

現在可以直接透過 URL 存取檔案:
```
http://localhost:5000/uploads/abc123.jpg
```
