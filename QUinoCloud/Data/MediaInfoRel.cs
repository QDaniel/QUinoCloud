using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QUinoCloud.Data
{
    public class MediaInfoRel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Position { get; set; }

        [Required]
        public int CatalogId { get; set; }

        public virtual MediaCatalogInfo? Catalog { get; set; }
        [Required]
        public int MediaId { get; set; }
        public virtual MediaInfo? Media { get; set; }
    }
}