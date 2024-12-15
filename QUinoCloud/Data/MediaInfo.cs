using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QUinoCloud.Data
{

    public class MediaInfo : IRfidCmd
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Album { get; set; } = string.Empty;
        public int TrackNr { get; set; } = 0;
        public string? Image { get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
        public TimeSpan? Duration { get; set; }

        public string? OwnerId { get; set; } = string.Empty;
        public virtual IdentityUser? Owner { get; set; }

        public string? DisplayTitle() {
            if (String.IsNullOrWhiteSpace(Album)) return Title;
            return $"{Album} - {Title}".Trim();
        }
        public Uri BuildUri(HttpContext ctx) {
            if (Url.Contains("://")) return new Uri(Url);
            return ctx.Request.GetUri($"/dl/media/{OwnerId}/{Url}");
        }
    }
}