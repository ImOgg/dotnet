## 課程進度到取得Messages
https://www.udemy.com/course/build-an-app-with-aspnet-core-and-angular-from-scratch/learn/lecture/51129535#overview

## cloudinary 
- 類似 S3 MinIO ，但這個可以更優化圖片的儲存或是輸出

## 新增功能步驟
1. ✅ 建立 Entity → API/Entities/Post.cs
2. ✅ 加到 DbContext → AppDbContext 新增 DbSet<Post>
3. ✅ 跑 Migration → dotnet ef migrations add AddPostEntity
4. ✅ 建立 DTO → CreatePostDTO、PostDTO
5. ✅ 建立 Repository → IPostRepository + PostRepository
   - 順序：先定義 IPostRepository 介面（規格），再寫 PostRepository 實作（細節）
6. ✅ 建立 Controller → PostsController（GET、POST 端點）

## Repository 模式 vs 直接用 DbContext

| | Repository 模式（PostsController） | 直接用 DbContext（PostsController2） |
|---|---|---|
| 注入 | `IPostRepository postRepository` | `AppDbContext context` |
| 查詢 | `postRepository.GetPostsAsync()` | `context.Posts.Include(...).ToListAsync()` |
| 新增 | `postRepository.AddPostAsync(post)` + `SaveAllAsync()` | `context.Posts.Add(post)` + `SaveChangesAsync()` |
| 需要的檔案 | 4 個（Interface、Repository、Controller、DI 註冊） | 1 個（只有 Controller） |
| 邏輯重用 | 集中在 Repository，多個 Controller 可共用 | 需要複製貼上 |
| 單元測試 | 容易 mock（依賴介面） | 難 mock |
| 適合情境 | 正式專案、多處共用查詢邏輯 | 學習、小專案、快速開發 |

## ⚠️ 尚未完成（遺漏的步驟）

### 1. AppUser 缺少導航屬性
- 檔案：`API/Entities/AppUser.cs`
- 需要加上：`public ICollection<Post> Posts { get; set; } = [];`
- 原因：讓 EF Core 知道 AppUser 與 Post 是一對多關係（一個 User 可有多篇 Post）
- ⚠️ 加完後需要跑新的 Migration：`dotnet ef migrations add AddPostsNavigationToUser`

### 2. Program.cs 缺少 DI 註冊
- 檔案：`API/Program.cs`
- 需要加上：`builder.Services.AddScoped<IPostRepository, PostRepository>();`
- 原因：沒有這行，PostsController 注入 IPostRepository 時會在執行期間報錯（找不到對應的服務）

