using System.ComponentModel.DataAnnotations;

namespace SentimentHub.Web.Models;

public class VerifyCodeViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Doğrulama Kodu")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 haneli olmalıdır.")]
    public string Code { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
