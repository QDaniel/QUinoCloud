using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Classes;
using QUinoCloud.Data;
using QUinoCloud.Utils;
using QUinoCloud.Utils.Extensions;
using TagLib.Asf;

namespace QUinoCloud.Pages.Manage.Medias
{
    public class EditModel(AppDbContext context, MediaDownloader hc) : PageModel
    {
        public string[] AllowedFileExt = [".mp3", ".flac", ".ogg"];

        [BindProperty]
        public MediaInfo Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id = 0)
        {
            if (id < 0) return NotFound();

            var entity = id != 0 ? await context.MyMedias(HttpContext).FirstOrDefaultAsync(m => m.Id == id) : new();
            if (entity == null) return NotFound();

            Entity = entity;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            context.Init(HttpContext);

            var entity = Entity.Id != 0 ? await context.MyMedias().FirstOrDefaultAsync(m => m.Id == Entity.Id) : new();
            if (entity == null) return NotFound();

            entity.TrackNr = Entity.TrackNr;
            entity.Image = Entity.Image;
            entity.Title = Entity.Title;
            entity.Album = Entity.Album;
            entity.Url = Entity.Url;

            entity.SetOwner(HttpContext);
            if (entity.Id == 0)
            {
                if (entity.Url.StartsWith("https://") || entity.Url.StartsWith("http://"))
                {
                    var uri = new Uri(entity.Url);
                    var fileExt = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
                    var fileName = Path.GetFileName(uri.AbsolutePath).ToLowerInvariant();
                    if (AllowedFileExt.Contains(fileExt))
                    {
                        try
                        {
                            var dir = context.MyMediaDir(HttpContext);

                            using var dataStrm = await hc.DownloadAsync(entity.Url);
                            if (dataStrm != null)
                            {
                                var flHashB = Utils.Crypto.BuildHashBytes(Utils.Crypto.AlgoSHA1, dataStrm);
                                var flHash = Utils.Crypto.ConvertHash(flHashB);
                                var guid = Base32.ToBase32String(flHashB).ToLower();
                                var pathData = Path.Combine(dir, guid + fileExt);

                                byte[]? picData = null;
                                string? picExt = null;
                                using (var meta = TagLib.File.Create(new StreamFileAbstraction(fileName, new ResetStreamWrapper(dataStrm))))
                                {
                                    if (meta.PossiblyCorrupt) return BadRequest();
                                    if (meta.Properties.Duration.TotalSeconds < 10) return BadRequest();
                                    if (meta.Properties.Duration.TotalHours > 3) return BadRequest();
                                    if (!meta.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Audio)) return BadRequest();
                                    if (meta.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video)) return BadRequest();

                                    var pic = meta.Tag.Pictures.FirstOrDefault();
                                    if (pic != null)
                                    {
                                        picData = pic.Data.ToArray();
                                        picExt = Mimetype.GetExtensionFromMimeType(pic.MimeType)?.ToLowerInvariant();
                                    }

                                    if (entity.TrackNr <= 0) entity.TrackNr = (int)meta.Tag.Track;
                                    if (string.IsNullOrWhiteSpace(entity.Album)) entity.Album = meta.Tag.Album;
                                    if (string.IsNullOrWhiteSpace(entity.Title))
                                    {
                                        entity.Title = meta.Tag.Title;
                                        if (!string.IsNullOrWhiteSpace(meta.Tag.FirstAlbumArtist)) entity.Title = meta.Tag.FirstAlbumArtist + " - " + entity.Title;
                                    }
                                    entity.Duration = meta.Properties.Duration;
                                    entity.FileHash = flHash;
                                }


                                dataStrm.Reset();
                                using (var strm = System.IO.File.OpenWrite(pathData))
                                {
                                    await dataStrm.CopyToAsync(strm);
                                }
                                entity.Url = Path.GetFileName(pathData);

                                if (picData != null && picExt != null)
                                {
                                    var hash = Utils.Crypto.BuildHashBytes(Utils.Crypto.AlgoSHA1, picData);
                                    var hash32 = Base32.ToBase32String(hash).ToLower();

                                    var pathImg = Path.Combine(dir, hash32 + picExt);
                                    if (!System.IO.File.Exists(pathImg))
                                    {
                                        using var strm = System.IO.File.OpenWrite(pathImg);
                                        using var picStrm = new MemoryStream(picData);
                                        await picStrm.CopyToAsync(strm);
                                    }
                                    entity.Image = Path.GetFileName(pathImg);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }


                context.Add(entity);
            }

            await context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
