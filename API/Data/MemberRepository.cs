using API.Interfaces;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

// 【Repository 模式 - 為什麼要有這個類別？】
// 如果 Controller 直接操作 DbContext，會有幾個問題：
// 1. 查詢邏輯散落在各個 Controller，重複且難以維護
// 2. Controller 與 EF Core 緊耦合，難以替換或測試
// 3. 違反單一職責原則（Controller 同時負責路由處理和資料存取）
//
// MemberRepository 把所有「會員資料存取邏輯」集中管理：
// Controller ──依賴介面──> IMemberRepository ──實作──> MemberRepository ──操作──> DbContext
//
// Primary Constructor 寫法（C# 12）：直接在類別名稱後注入 AppDbContext，
// 等同於宣告私有欄位 + 建構函式的傳統寫法。
public class MemberRepository(AppDbContext context) : IMemberRepository
{
    // 為什麼用 FindAsync 而不是 FirstOrDefaultAsync？
    // FindAsync 會先查 EF Core 的變更追蹤快取（已載入到記憶體的實體），
    // 找到就直接回傳，不發出 SQL；找不到才查資料庫。
    // 適合用 primary key 查詢的情境，比 FirstOrDefaultAsync 效能更好。
    // FindAsync 不支援 Include，member.User 是 null，所以改用 SingleOrDefaultAsync 搭配 Include。
    public async Task<Member?> GetMemberByIdAsync(string id)
    {   
        return await context.Members
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    // 為什麼用 ToListAsync 而不是 AsEnumerable？
    // ToListAsync 會一次把所有資料載入記憶體，後續操作在記憶體中執行。
    // AsEnumerable 也是在記憶體中操作，但不非同步。
    // 兩者都不同於 IQueryable，IQueryable 的篩選會轉成 SQL 在資料庫執行（效能更好）。
    // 這裡回傳所有會員清單，沒有進一步篩選，所以 ToListAsync 是合適的選擇。
    public async Task<IReadOnlyList<Member>> GetMembersAsync()
    {
        return await context.Members.ToListAsync();
    }

    // 為什麼用 Where + SelectMany 而不是先找 Member 再用 member.Photos？
    // 直接在資料庫層面完成篩選和展開，只回傳需要的 Photos，
    // 不會把整個 Member 實體（包含其他欄位）載入記憶體，減少資料傳輸量。
    //
    // SelectMany：將一對多關係「攤平」——
    // 原本是 IQueryable<Member>，每個 Member 有 ICollection<Photo>，
    // SelectMany 把所有 Member 的 Photos 合併成單一 IQueryable<Photo>。
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

    // 為什麼不直接在這裡呼叫 SaveChangesAsync？
    // Unit of Work 模式：一個請求（request）可能需要修改多個實體，
    // 把所有修改累積起來，最後一次呼叫 SaveAllAsync() 統一提交，
    // 確保「全部成功」或「全部回滾」，維持資料一致性。
    // 如果每次 Update 都立刻存，就失去了這個原子性保證。
    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }

    // 為什麼要 Include(x => x.User)？
    // EF Core 預設使用「延遲載入」或「不載入」關聯資料（視設定而定）。
    // Member 和 User 是一對一關聯，如果不 Include，
    // 查出來的 Member 物件的 User 屬性會是 null。
    // Include 告訴 EF Core 用 JOIN 把關聯的 User 一起查出來。
    public async Task<Member?> GetMemberByUsernameAsync(string username)
    {
        return await context.Members
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.User.Email == username);
    }
}
