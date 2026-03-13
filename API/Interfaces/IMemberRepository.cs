using API.Entities;

namespace API.Interfaces;

// 【為什麼要定義介面（Interface）而不直接用實作類別？】
// 核心原則：依賴抽象，不依賴具體實作（Dependency Inversion Principle）。
//
// 好處 1 — 可測試性：
//   Controller 只認識這個介面，測試時可以注入假的 Mock 實作，
//   不需要連接真實資料庫，測試速度快且不污染資料。
//
// 好處 2 — 可替換性：
//   今天用 EF Core + MySQL，明天改成 Dapper 或換成 PostgreSQL，
//   只要新的實作符合這個介面，Controller 的程式碼完全不需要改。
//
// 好處 3 — 關注點分離：
//   介面定義「能做什麼」（合約），實作類別定義「怎麼做」（細節）。
//   Controller 只需要知道「能做什麼」，不需要知道底層怎麼操作資料庫。
public interface IMemberRepository
{
    // 為什麼是 void 而不是 Task？
    // Update 只是改變 EF Core 記憶體中追蹤的狀態（EntityState.Modified），
    // 不涉及任何 I/O 操作，所以不需要 async/await，直接同步執行即可。
    // 真正寫入資料庫的動作在 SaveAllAsync() 裡才發生。
    void Update(Member member);

    // 為什麼回傳 bool 而不是 int（受影響的資料列數）？
    // Controller 只需要知道「有沒有成功」，不需要知道具體影響了幾筆，
    // bool 讓呼叫端的 if 判斷更直覺：if (await SaveAllAsync()) return NoContent();
    Task<bool> SaveAllAsync();

    // 為什麼回傳 IReadOnlyList<T> 而不是 List<T>？
    // 向外部傳達「這個集合不應該被修改」的意圖（防禦性設計）。
    // 呼叫端拿到後無法呼叫 Add()、Remove() 等修改方法，避免意外變動。
    Task<IReadOnlyList<Member>> GetMembersAsync();

    // 為什麼回傳 Member?（可為 null）？
    // 表示這個方法可能找不到對應的 Member（id 不存在），
    // 用 ? 強制呼叫端（Controller）在使用前做 null 檢查，避免 NullReferenceException。
    Task<Member?> GetMemberByIdAsync(string id);

    Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);

    Task<Member?> GetMemberByUsernameAsync(string id);
}
