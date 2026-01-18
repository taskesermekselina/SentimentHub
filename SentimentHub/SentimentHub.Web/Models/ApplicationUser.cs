using Microsoft.AspNetCore.Identity;

namespace SentimentHub.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
    
    // OTP Fields
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeExpiresAt { get; set; }
}
