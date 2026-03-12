// 【引用說明】
// API.Data      → AppDbContext（EF Core 資料庫上下文）
// API.Services  → TokenService（JWT Token 產生的具體實作）
// API.Interfaces → ITokenService（TokenService 所實作的介面）
// Microsoft.EntityFrameworkCore → UseSqlite() 等 EF Core 擴充方法
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ===================== 服務註冊（DI 容器）=====================

builder.Services.AddControllers();

// 註冊 EF Core DbContext，使用 SQLite 資料庫
// 連線字串從 appsettings.json 的 ConnectionStrings:DefaultConnection 讀取
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 【Repository 服務】
// 將 IMemberRepository 介面對應到 MemberRepository 具體實作。
// Scoped = 每次 HTTP 請求建立一個新實例。
// Controller 只需宣告 IMemberRepository，DI 框架會自動注入 MemberRepository。
builder.Services.AddScoped<IMemberRepository, MemberRepository>();

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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"] ?? throw new ArgumentException("Token key is missing in configuration");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // 驗證簽名金鑰
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)), // 用來驗證簽名的金鑰
            ValidateIssuer = false, // 不驗證發行者
            ValidateAudience = false, // 不驗證受眾
            ValidateLifetime = true, // 驗證過期時間
        };
    });
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

// 開發環境下顯示詳細錯誤頁面，方便除錯
// 但在Properties/launchSettings.json中將環境變數設定為 Production，所以這段程式碼會被註解掉，避免在生產環境中暴露詳細錯誤資訊
// app.UseDeveloperExceptionPage();

app.UseMiddleware<ExceptionMiddleware>(); // 全局異常處理中介軟體，捕獲未處理的異常並返回統一格式的錯誤響應

// 啟用 CORS（跨來源資源共享），允許前端應用程式從不同的來源訪問 API
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000", "https://localhost:3000"));

// 認證與授權中介軟體 這兩個中介軟體的順序很重要，必須先 UseAuthentication()，再 UseAuthorization()，才能正確處理 JWT Token 的驗證與授權
app.UseAuthentication();
app.UseAuthorization();

// 將請求路由到對應的 Controller Action
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context= services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync(); // 自動執行資料庫遷移
    await Seed.SeedData(context); // 執行資料庫種子方法，初始化測試資料
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration or seeding");
    throw;
}

app.Run();
