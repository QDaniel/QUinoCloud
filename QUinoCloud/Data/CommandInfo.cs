using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QUinoCloud.Data
{
    public interface IEditStamp
    {
        void RenewEditStamp();
    }
    public interface IRfidCmd : IImageable, ITitled
    {

    }
    public interface ITitled 
    {
        string Title { get; }
    }
    public interface IImageable : IOwnable
    {
        string? Image { get; }
    }
    public interface IOwnable
    {
        string? OwnerId { get; set; }
        IdentityUser? Owner { get; set; }
    }
    public class CommandInfo: IRfidCmd
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;

        [Required]
        public string Command { get; set; } = string.Empty;

        public string? OwnerId { get; set; } = string.Empty;
        public virtual IdentityUser? Owner { get; set; }

    }
}