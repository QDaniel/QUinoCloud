using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;
using QUinoCloud.Utils;
using System.Xml.Linq;

namespace QUinoCloud.Pages.Manage.Medias
{
    [IgnoreAntiforgeryToken]
    [DisableRequestSizeLimit]
    public class UploadModel(AppDbContext context) : PageModel
    {
        public int MaxFileSize = 100; //MB
        public IActionResult OnGet() => Page();

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.

        public async Task<IActionResult> OnPostAsync(IFormFile Files)
        {
            if (Files == null) return BadRequest();
            if (Files.Length > MaxFileSize * 1048576) return BadRequest();
            if (!Files.ContentType.StartsWith("audio/")) return BadRequest();
            if (!ModelState.IsValid)
            {
                return Page();
            }
            string flHash = string.Empty;
            byte[] flHashB = [];
            using (var fldata = Files.OpenReadStream())
            {
                flHashB = Utils.Crypto.BuildHashBytes(Utils.Crypto.AlgoSHA1, fldata);
                flHash = Utils.Crypto.ConvertHash(flHashB);
            }

            var mediaCheck = await context.MyMedias(HttpContext).FirstOrDefaultAsync(o => o.FileHash == flHash);
            if (mediaCheck != null) return BadRequest();

            var meta = TagLib.File.Create(new FormFileAbstraction(Files));
            if (meta.PossiblyCorrupt) return BadRequest();
            if (meta.Properties.Duration.TotalSeconds < 10) return BadRequest();
            if (meta.Properties.Duration.TotalHours > 3) return BadRequest();
            if (!meta.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Audio)) return BadRequest();
            if (meta.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video)) return BadRequest();


            var dir = context.MyMediaDir(HttpContext);
            var guid = Base32.ToBase32String(flHashB).ToLower();

            var pathData = Path.Combine(dir, guid + Path.GetExtension(Files.FileName).ToLowerInvariant());


            using (var strm = System.IO.File.OpenWrite(pathData))
            {
                await Files.CopyToAsync(strm);
            }

            var entity = new MediaInfo();
            entity.TrackNr = (int)meta.Tag.Track;
            entity.Title = meta.Tag.Title;
            if (!string.IsNullOrWhiteSpace(meta.Tag.FirstAlbumArtist)) entity.Title = meta.Tag.FirstAlbumArtist + " - " + entity.Title;
            entity.Album = meta.Tag.Album;
            entity.Duration = meta.Properties.Duration;
            entity.Url = Path.GetFileName(pathData);
            entity.FileHash = flHash;
            var pic = meta.Tag.Pictures.FirstOrDefault();
            if (pic != null)
            {
                var picdata = pic.Data.ToArray();
                var hash = Utils.Crypto.BuildHashBytes(Utils.Crypto.AlgoSHA1, picdata);
                var hash32 = Base32.ToBase32String(hash).ToLower();

                var pathImg = Path.Combine(dir, hash32 + Utils.Mimetype.GetExtensionFromMimeType(pic.MimeType).ToLowerInvariant());
                if (!System.IO.File.Exists(pathImg))
                {
                    using var strm = System.IO.File.OpenWrite(pathImg);
                    using var picStrm = new MemoryStream(picdata);
                    await picStrm.CopyToAsync(strm);
                }
                entity.Image = Path.GetFileName(pathImg);
            }

            entity.SetOwner(HttpContext);
            if (entity.Id == 0) context.Add(entity);

            await context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

    public class FormFileAbstraction : TagLib.File.IFileAbstraction
    {
        public FormFileAbstraction(IFormFile form)
        {
            Name = form.FileName;
            ReadStream = form.OpenReadStream();
        }
        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

        public void CloseStream(Stream stream)
        {
            ReadStream?.Dispose();
        }
    }
    public class StreamFileAbstraction : TagLib.File.IFileAbstraction
    {
        public StreamFileAbstraction(string filename, Stream data )
        {
            Name = filename;
            ReadStream = data;
        }
        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

        public void CloseStream(Stream stream)
        {
            ReadStream?.Dispose();
        }
    }
}
