using API.Entities;

namespace API.Interfaces;

// 【Repository 模式 - 介面定義】
// Repository 模式的核心概念：將資料存取邏輯與業務邏輯分離。
// Controller 只依賴這個介面，不直接碰 DbContext，
// 好處是：測試時可以換成假的實作（Mock），不需要真實資料庫。
public interface IMemberRepository
{
    // 標記實體為已修改狀態，下次 SaveAllAsync() 時才會真正寫入資料庫
    void Update(Member member);

    // 呼叫 DbContext.SaveChangesAsync()，將所有暫存的變更一次寫入資料庫
    // 回傳 true 表示至少有一筆資料被影響（成功儲存）
    Task<bool> SaveAllAsync();

    // 取得所有會員（唯讀列表，防止外部意外修改集合）
    Task<IReadOnlyList<Member>> GetMembersAsync();

    // 依 id 查詢單一會員，找不到時回傳 null（? 表示可為 null）
    Task<Member?> GetMemberByIdAsync(string id);

    // 查詢指定會員的所有照片
    Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId);
}
