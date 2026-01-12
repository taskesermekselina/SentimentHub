using System.ComponentModel.DataAnnotations;

namespace SentimentHub.Web.Models;

public class Business
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Url]
    public string GoogleMapsUrl { get; set; } = string.Empty;

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }

    // Navigation
    public ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();
}
