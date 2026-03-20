using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

// ===== 寫法一：傳統建構函式（C# 9 以前，較直觀）=====
// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//     {
//     }
//
//     public DbSet<AppUser> Users { get; set; }
// }

// ===== 寫法二：Primary Constructor（C# 12，較簡潔）=====
// public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
// {
//     public DbSet<AppUser> Users { get; set; }
// }

// ===== 目前使用：Primary Constructor 寫法（C# 12）=====
// DbContextOptions<AppDbContext> 由 DI 容器注入，包含連線字串、資料庫提供者等設定
// : DbContext(options) 將 options 傳給父類別 DbContext 的建構函式
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet<T> 對應資料庫中的一張資料表
    // EF Core 會依照屬性名稱（Users、Members...）自動命名資料表
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<MemberLike> Likes { get; set; }

    public DbSet<Post> Posts { get; set; }

    // OnModelCreating：EF Core 建立模型時呼叫，可在此自訂資料表結構、關聯、轉換器等
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // 務必呼叫父類別，確保內建設定不被覆蓋

        // ===== MemberLike 的複合主鍵設定 =====
        modelBuilder.Entity<MemberLike>()
            .HasKey(k => new { k.SourceUserId, k.TargetMemberId }); // 設定複合主鍵

        modelBuilder.Entity<MemberLike>()
            .HasOne(s => s.SourceMember) // MemberLike 與 SourceMember 是多對一關係
            .WithMany(m => m.LikedMembers) // Member 有多個 LikedMembers（喜歡的會員列表）
            .HasForeignKey(s => s.SourceUserId) // 外鍵是 SourceUserId
            .OnDelete(DeleteBehavior.Cascade); // 刪除 SourceMember 時，同步刪除相關 MemberLike

        modelBuilder.Entity<MemberLike>()
            .HasOne(s => s.TargetMember) // MemberLike 與 TargetMember 是多對一關係
            .WithMany(m => m.LikedByMembers) // Member 有多個 LikedByMembers（被喜歡的會員列表）
            .HasForeignKey(s => s.TargetMemberId) // 外鍵是 TargetMemberId
            .OnDelete(DeleteBehavior.NoAction); // 刪除 TargetMember 時，同步刪除相關 MemberLike


        // ===== DateTime UTC 轉換器 =====
        // 問題背景：MySQL 儲存 DateTime 時不帶時區資訊，
        //          EF Core 讀取後 Kind 預設為 Unspecified，
        //          導致 .NET 比較或序列化時產生時區錯誤。
        // 解法：讀取後統一標記為 UTC（不改變數值，只修正 Kind）
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v,                                              // 寫入 DB：不做轉換
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)      // 讀取 DB：標記為 UTC
        );

        // 套用至所有 Entity 的所有 DateTime 屬性
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }
    }
}