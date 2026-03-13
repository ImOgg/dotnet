namespace API.DTOs
{
    // 【為什麼用 DTO 而不直接用 Member 實體？】
    // 1. 安全性：Member 實體包含資料庫所有欄位（如 Id、關聯的 User），
    //    直接讓外部傳入整個實體，可能被惡意篡改不應該被修改的欄位（Mass Assignment 攻擊）。
    //    DTO 只暴露「允許使用者修改」的欄位，其餘欄位在後端控制。
    //
    // 2. 解耦：資料庫結構改變時（加欄位、改名），不影響 API 介面；
    //    API 介面改變時，也不需要同步改資料庫欄位。
    //
    // 3. 驗證邊界清晰：DTO 是外部輸入的門口，可以在這裡加 [Required]、[MaxLength] 等驗證屬性，
    //    不會污染代表資料庫結構的 Entity 類別。
    public class MemberUpdateDTO
    {
        // 為什麼全部用 string?（可為 null）？
        // 因為這是「部分更新」（Partial Update）的設計：
        // 使用者不一定每次都要更新所有欄位，null 表示「這個欄位不更新」。
        // 在 Controller 裡用 ?? 運算子判斷：若 DTO 的值為 null，就保留原本的值。
        // 例：member.City = memberUpdateDTO.City ?? member.City;
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}