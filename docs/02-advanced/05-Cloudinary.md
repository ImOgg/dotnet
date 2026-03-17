# Cloudinary 圖片上傳整合

## 概述

Cloudinary 是雲端圖片/影片管理平台，提供上傳、儲存、轉換、CDN 發布等功能。
本專案使用 Cloudinary 儲存會員大頭照，避免將圖片存放在本地伺服器。

---

## 安裝

```bash
dotnet add package CloudinaryDotNet
```

---

## 設定流程

### 1. 設定檔（appsettings.json）

```json
"CloudinarySettings": {
  "CloudName": "你的 CloudName",
  "ApiKey": "你的 ApiKey",
  "ApiSecret": "你的 ApiSecret"
}
```

> **注意**：`appsettings.json` 不應提交到 Git（含有 API 金鑰），請加入 `.gitignore`。

---

### 2. 設定類別（Options Pattern）

```
API/Helpers/CloudinarySetting.cs
```

```csharp
namespace API.Helpers;

public class CloudinarySettings
{
    public required string CloudName { get; set; }
    public required string ApiKey { get; set; }
    public required string ApiSecret { get; set; }
}
```

**為什麼用 Options Pattern（`IOptions<T>`）？**
- 將設定值強型別化，避免在各處用 `Configuration["key"]` 魔術字串
- 支援 DI 注入，易於測試與替換

---

### 3. 服務介面

```
API/Interfaces/IPhotoService.cs
```

```csharp
using CloudinaryDotNet.Actions;

namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> UploadPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}
```

---

### 4. 服務實作

```
API/Services/PhotoService.cs
```

```csharp
public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "da-ang20"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
```

**重點說明**：
- `Transformation`：上傳時自動裁切為 500×500，`Gravity("face")` 讓裁切以臉部為中心
- `Folder`：指定 Cloudinary 儲存資料夾名稱
- `SecureUrl`：上傳完成後，從 `ImageUploadResult.SecureUrl.AbsoluteUri` 取得 HTTPS 圖片網址
- `PublicId`：刪除時使用，是 Cloudinary 對每張圖片的唯一識別碼

---

### 5. 在 Program.cs 註冊服務

```csharp
// 綁定 Cloudinary 設定到 CloudinarySettings 類別
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

// 將 IPhotoService 介面對應到 PhotoService 具體實作
builder.Services.AddScoped<IPhotoService, PhotoService>();
```

---

### 6. DTO

```
API/DTOs/PhotoUploadDTO.cs
```

```csharp
namespace API.DTOs;

public class PhotoUploadDTO
{
    public required IFormFile File { get; set; }
}
```

**為什麼用 `IFormFile`？**
- ASP.NET Core 內建的檔案上傳型別
- 配合 `[FromForm]` 接收 `multipart/form-data` 請求

---

## Controller 端點

```
POST /api/members/add-photo
```

```csharp
[HttpPost("add-photo")]
public async Task<ActionResult<Photo>> AddPhoto([FromForm] PhotoUploadDTO dto)
{
    var member = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
    if (member == null) return BadRequest("member not found");

    var result = await photoService.UploadPhotoAsync(dto.File);
    if (result.Error != null) return BadRequest(result.Error.Message);

    var photo = new Photo
    {
        Url = result.SecureUrl.AbsoluteUri,   // Cloudinary HTTPS 圖片網址
        PublicId = result.PublicId,            // 刪除時需要
        MemberId = User.GetMemberId(),
    };

    // 第一張圖片自動設為大頭照
    if (member.ImageUrl == null)
    {
        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;
    }

    member.Photos.Add(photo);

    if (await memberRepository.SaveAllAsync()) return photo;

    return BadRequest("Failed to add photo");
}
```

**設計重點**：
- `[FromForm]`：告訴 ASP.NET Core 從 `multipart/form-data` 解析 DTO，而非 JSON body
- `User.GetMemberId()`：從 JWT Token 取得當前使用者 ID，確保只能上傳自己的照片
- 第一張照片自動同步更新 `Member.ImageUrl` 與 `AppUser.ImageUrl`

---

## 整體流程圖

```
前端 (multipart/form-data)
    │
    ▼
MembersController.AddPhoto()
    │  取出 JWT Token 中的 memberId
    │
    ▼
PhotoService.UploadPhotoAsync()
    │  將圖片串流上傳到 Cloudinary
    │
    ▼
Cloudinary 回傳 ImageUploadResult
    │  含 SecureUrl、PublicId
    │
    ▼
建立 Photo 實體並儲存到資料庫
    │
    ▼
回傳 Photo 物件給前端
```

---

## 相關檔案索引

| 檔案 | 用途 |
|------|------|
| `API/Helpers/CloudinarySetting.cs` | Options Pattern 設定類別 |
| `API/Interfaces/IPhotoService.cs` | 服務介面定義 |
| `API/Services/PhotoService.cs` | Cloudinary 上傳/刪除實作 |
| `API/DTOs/PhotoUploadDTO.cs` | 接收上傳請求的 DTO |
| `API/Controllers/MembersController.cs` | `POST add-photo` 端點 |
| `API/Program.cs` | 服務與設定註冊 |
