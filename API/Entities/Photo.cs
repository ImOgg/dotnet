using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace API.Entities;

public class Photo
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public string? PublicId { get; set; }
    public string Memberid { get; set; } = null!;

    // Navigation property
    [ForeignKey("Memberid")]
    public Member Member { get; set; } = null!;
}
