using API.Interfaces;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// 【Repository 模式 - 具體實作】
// Primary Constructor 寫法（C# 12）：直接在類別名稱後注入 AppDbContext，
// 等同於宣告私有欄位 + 建構函式的傳統寫法。
// 實作 IMemberRepository 介面，所有方法都透過 context 操作資料庫。
public class MemberRepository(AppDbContext context) : IMemberRepository
{
    // FindAsync：先查 EF Core 追蹤快取，再查資料庫，效能較好
    // 適合用 primary key 查詢單筆資料的情境
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members.FindAsync(id);
    }

    // ToListAsync：將整個 DbSet 轉成 List，一次載入所有資料
    public async Task<IReadOnlyList<Member>> GetMembersAsync()
    {
        return await context.Members.ToListAsync();
    }

    // Where：篩選出指定 memberId 的會員
    // SelectMany：將會員的 Photos 集合「攤平」成單一序列（一對多展開）
    public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId)
    {
        return await context.Members.Where(m => m.Id == memberId)
            .SelectMany(m => m.Photos)
            .ToListAsync();
    }

    // SaveChangesAsync 回傳受影響的資料列數，> 0 表示有成功寫入
    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    // 將實體狀態設為 Modified，告訴 EF Core「這筆資料有被修改」
    // 下次呼叫 SaveAllAsync() 時，EF Core 才會產生 UPDATE SQL
    // 注意：這裡不需要 async，因為只是改記憶體中的狀態，不涉及 I/O
    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }
}
