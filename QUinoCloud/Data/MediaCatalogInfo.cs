using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QUinoCloud.Data
{
    public class MediaCatalogInfo : IRfidCmd
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;

        public string OwnerId { get; set; } = string.Empty;
        public virtual IdentityUser? Owner { get; set; }

        [MinLength(1)]
        public virtual List<MediaInfoRel>? Medias { get; set; }
    }
}