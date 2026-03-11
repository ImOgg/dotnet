// 【引用說明】
// API.Data      → AppDbContext（EF Core 資料庫上下文）
// API.Services  → TokenService（JWT Token 產生的具體實作）
// API.Interfaces → ITokenService（TokenService 所實作的介面）
// Microsoft.EntityFrameworkCore → UseSqlite() 等 EF Core 擴充方法
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ===================== 服務註冊（DI 容器）=====================

builder.Services.AddControllers();

// 註冊 EF Core DbContext，使用 SQLite 資料庫
// 連線字串從 appsettings.json 的 ConnectionStrings:DefaultConnection 讀取
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 【JWT Token 服務】
// 將 ITokenService 介面對應到 TokenService 具體實作，並以 Scoped 生命週期注入。
// Scoped = 每次 HTTP 請求建立一個新實例，請求結束後銷毀。
// Controller 只需宣告 ITokenService，DI 框架會自動注入 TokenService。
//
// 【前置需求 - NuGet 套件】
// dotnet add package System.IdentityModel.Tokens.Jwt
// dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
//
// 【前置需求 - appsettings.json】
// 需加入 JWT 金鑰設定（長度至少 64 字元）：
// "JWT": {
//   "Key": "your-super-secret-key-at-least-64-characters-long-xxxxxxxxxxxx"
// }
builder.Services.AddScoped<ITokenService, TokenService>();

// ===================== 中介軟體（Middleware）管線 =====================

var app = builder.Build();

// Swagger UI（開發環境下啟用，課程暫時移除）
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// 不是 HTTPS 就不需要重定向
// app.UseHttpsRedirection();

// 認證與授權中介軟體（課程暫時移除，之後加入 JWT 驗證後需啟用）
// app.UseAuthentication();
// app.UseAuthorization();

// 將請求路由到對應的 Controller Action
app.MapControllers();

app.Run();
