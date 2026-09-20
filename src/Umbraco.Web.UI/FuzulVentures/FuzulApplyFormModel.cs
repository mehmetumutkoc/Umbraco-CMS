using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulApplyFormModel
{
    [Required]
    public string? StartupName { get; set; }

    [Required]
    public string? Website { get; set; }

    public string? Summary { get; set; }

    public string? Industry { get; set; }

    [Required]
    public string? FounderName { get; set; }

    [Required]
    [EmailAddress]
    public string? FounderEmail { get; set; }

    [Required]
    public IFormFile? Presentation { get; set; }
}
