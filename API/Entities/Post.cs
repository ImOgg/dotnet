namespace API.Entities
{
    // 文章實體（Post 層）
    // 儲存使用者發布的文章內容
    // 與 AppUser 是多對一關係（一個使用者可發多篇文章）
    public class Post
    {
        // 主鍵：int 自增，由資料庫自動產生
        public int Id { get; set; }

        // 文章標題，required 表示建立時必須提供
        public required string Title { get; set; }

        // 文章內容，required 表示建立時必須提供
        public required string Content { get; set; }

        // 發文時間：建立物件時自動填入 UTC 時間
        // 使用 UtcNow 而非 Now，避免時區問題
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 外鍵：指向 AppUser.Id（string UUID）
        // EF Core 命名慣例：欄位名 = 導航屬性名(User) + Id → 自動識別為 FK，不需要 [ForeignKey]
        // 型別必須與 AppUser.Id 一致（string），否則 EF Core 會報型別不相容錯誤
        public required string UserId { get; set; }

        // 導航屬性：透過 UserId 關聯到對應的 AppUser
        // null! 告訴編譯器：EF Core 載入時一定會賦值，不需要警告
        public AppUser User { get; set; } = null!;
    }
}