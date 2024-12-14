using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUinoCloud.Data
{
    public class RfidTag : IImageable, IEditStamp
    {
        private string serialNr = string.Empty;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(20)]
        [RegularExpression("^[0-9A-F]{8,20}$")]
        public string SerialNr
        {
            get => serialNr;
            set
            {
                var ret = value.ToUpperInvariant().Replace(":", "").Replace(" ", "").Trim();
                if (ret.Any(c => !"0123456789ABCDEF".Contains(c))) throw new InvalidDataException($"{nameof(serialNr)}: {ret} is not a hex string");
                serialNr = ret;
            }
        }

        public string? Title { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;

        public string? OwnerId { get; set; } = string.Empty;
        public virtual IdentityUser? Owner { get; set; }

        public int? CatalogId { get; set; }
        public virtual MediaCatalogInfo? Catalog { get; set; }

        public int? MediaId { get; set; }
        public virtual MediaInfo? Media { get; set; }

        public int? CommandId { get; set; }
        public virtual CommandInfo? Command { get; set; }
        public string EditStamp { get; set; } = Guid.NewGuid().ToString();

        [NotMapped]
        public RfidTagMode Mode
        {
            get
            {
                if (MediaId > 0) return RfidTagMode.Media;
                if (CatalogId > 0) return RfidTagMode.Catalog;
                if (CommandId > 0) return RfidTagMode.Cmd;
                return RfidTagMode.None;
            }
        }
        public IRfidCmd? GetCmd()
        {
            if (MediaId > 0) return Media;
            if (CatalogId > 0) return Catalog;
            if (CommandId > 0) return Command;
            return null;
        }

        public void RenewEditStamp()
        {
            EditStamp = Guid.NewGuid().ToString();
        }
    }


    public enum RfidTagMode
    {
        None,
        Media,
        Catalog,
        Cmd
    }
}
