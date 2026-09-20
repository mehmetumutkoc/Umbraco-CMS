using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulContactFormModel
{
    [Required]
    public string? FullName { get; set; }

    [Required]
    public string? Phone { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    public IFormFile? Attachment { get; set; }
}
