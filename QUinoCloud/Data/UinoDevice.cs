using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QUinoCloud.Data;
using Microsoft.AspNetCore.Identity;

public class UinoDevice : IImageable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MinLength(12)]
    [MaxLength(12)]
    [RegularExpression("^[0-9A-F]{12}$")]
    public string MAC { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Image { get; set; } = string.Empty;

    public string? OwnerId { get; set; } = string.Empty;
    public IdentityUser? Owner { get; set; }

    public string? LastInfo { get; set; } = string.Empty;

    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
}