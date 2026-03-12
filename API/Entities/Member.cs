using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Entities;

public class Member
{   
    // null! 是不可為null的提示，告訴編譯器這個屬性在使用前一定會被賦值（例如透過 EF Core 的關聯載入），所以不需要警告。
    public string id { get; set; } = null!;

    public DateOnly DateofBirth { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime LastActive { get; set; } = DateTime.Now;
    
    public required string Gender { get; set; } 

    public string Description { get; set; } = null!;

    public required string City { get; set; } 

    public required string Country { get; set; } 

    // Navigation property
    [ForeignKey("id")]
    public AppUser User { get; set; }
}
