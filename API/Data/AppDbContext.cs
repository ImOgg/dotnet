using API.Entities;
using Microsoft.EntityFrameworkCore;

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

// 目前使用：傳統寫法
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
}
