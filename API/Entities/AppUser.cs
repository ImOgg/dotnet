namespace API.Entities;

public class AppUser
{
    // 主鍵：建立物件時自動產生唯一的 UUID 字串（e.g. "550e8400-e29b-41d4-a716-446655440000"）
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string DisplayName { get; set; }

    public required string Email { get; set; }

    public required byte[]? PasswordHash { get; set; }
    public required byte[]? PasswordSalt { get; set; }
}
