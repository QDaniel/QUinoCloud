using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;
using System.Net;
using System.Text;

namespace QUinoCloud.Controllers
{
    [Route("api/rfid/")]
    public class RfidController(AppDbContext context) : Controller
    {
        [AllowAnonymous]
        [Route("info/{tagSerial}")]
        public async Task<IActionResult> TagInfoAsync(string tagSerial)
        {
            tagSerial = tagSerial.ToUpperInvariant().Replace(":", "").Replace("-", "").Replace(" ", "").Trim();
            var tagInfo = await context.RfidCards
                .Include(o => o.Catalog)
                .Include(o => o.Media)
                .Include(o => o.Command)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.SerialNr == tagSerial);

            var mac = Request.Headers["X-Ident"].ToString();
            mac = mac.ToUpperInvariant().Replace(":", "").Replace("-", "").Replace(" ", "").Trim();
            var uino = (!string.IsNullOrWhiteSpace(mac) && mac.Length == 12) ? await context.Devices.FirstOrDefaultAsync(o => o.MAC == mac) : null;

            if (tagInfo?.GetCmd() == null)
            {
                if (string.IsNullOrWhiteSpace(tagInfo?.Title))
                {
                    if (uino != null)
                    {
                        if (tagInfo == null)
                        {
                            tagInfo = new RfidTag() { SerialNr = tagSerial };
                            context.RfidCards.Add(tagInfo);
                        }
                        tagInfo.OwnerId = uino.OwnerId;
                        await context.SaveChangesAsync();
                    }
                }
                return TagNotFound();
            }

            context.CurrentUserID = tagInfo.OwnerId;
            if (!string.IsNullOrWhiteSpace(mac) && mac.Length == 12)
            {
                if (uino == null)
                {
                    uino = new UinoDevice() { MAC = mac, OwnerId = tagInfo.OwnerId };
                    context.Devices.Add(uino);
                }
                uino.LastSeen = DateTimeOffset.Now;
                await context.SaveChangesAsync();
            }
            var reqetag = "\"" + tagInfo.EditStamp + "\"";
            HttpContext.Response.Headers.ETag = reqetag;

            if (Request.Headers.IfNoneMatch.Contains(reqetag))
                return StatusCode((int)HttpStatusCode.NotModified);

            var mode = tagInfo.Mode;
            var list = new List<string>();
            bool incomplete = false;
            if (mode == Data.RfidTagMode.Cmd)
            {
                list.Add("#CMD:" + (char)Convert.ToInt32(tagInfo.Command!.Command));
            }
            if (mode == Data.RfidTagMode.Media || (mode == Data.RfidTagMode.Catalog && tagInfo.Catalog?.Medias != null))
            {
                var medias = (mode == Data.RfidTagMode.Catalog) ? tagInfo.Catalog!.Medias!.Select(o => o.Media) : [tagInfo.Media];

                foreach (var item in medias)
                {
                    if (item == null) continue;
                    list.Add(string.Format("#EXTINF:{0},{1}", (int?)item.Duration?.TotalSeconds, item.DisplayTitle()));
                    var path = Path.Combine(context.MyMediaDir(HttpContext), item.Url);
                    var file = item.Duration != null ? new FileInfo(path) : null;
                    if (file?.Length > 0)
                    {
                        var tDir = Utils.Files.SanitizeFilename(string.IsNullOrWhiteSpace(item.Album) ? tagInfo.SerialNr : item.Album);
                        var bName = item.DisplayTitle();
                        if (string.IsNullOrEmpty(bName)) bName = Path.GetFileNameWithoutExtension(item.Url);
                        bName += Path.GetExtension(item.Url);
                        bName = Utils.Files.SanitizeFilename(bName);
                        list.Add(string.Format("#DL-FILE:/{0};{1};{2}", tDir, bName, file.Length));
                    }
                    list.Add(item!.BuildUri(HttpContext).ToString());
                }
            }
            return Content(string.Join("\n", list), "audio/x-mpegurl", Encoding.ASCII);
        }

        [AllowAnonymous]
        [Route("{tagSerial}/list.m3u")]
        public async Task<IActionResult> TagM3UAsync(string tagSerial)
        {
            tagSerial = tagSerial.ToUpperInvariant().Replace(":", "").Replace("-", "").Replace(" ", "").Trim();
            var tagInfo = await context.RfidCards
                .Include(o => o.Catalog)
                .Include(o => o.Media)
                .Include(o => o.Command)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.SerialNr == tagSerial);

            if (tagInfo == null) return NotFound();
            var mode = tagInfo.Mode;

            if (mode == Data.RfidTagMode.Cmd) return NotFound();

            var list = new List<string>();
            list.Add("#EXTM3U");
            if (mode == Data.RfidTagMode.Media)
            {
                list.Add(string.Format("#EXTINF:{0},{1}", tagInfo.Media!.Duration?.TotalSeconds, tagInfo.Media.DisplayTitle()));
                list.Add(tagInfo.Media.BuildUri(HttpContext).ToString());
            }
            if (mode == Data.RfidTagMode.Catalog && tagInfo.Catalog?.Medias != null)
            {
                foreach (var item in tagInfo.Catalog.Medias.OrderBy(o => o.Position).Select(o => o.Media))
                {
                    if (item == null) continue;
                    list.Add(string.Format("#EXTINF:{0},{1}", item.Duration?.TotalSeconds, item.DisplayTitle()));
                    list.Add(item.BuildUri(HttpContext).ToString());
                }
            }
            var useragent = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty;
            if (!useragent.Contains("Mozilla/")) list = list.Where(o => !o.StartsWith("#")).ToList();
            return Content(string.Join("\r\n", list), "audio/x-mpegurl", Encoding.ASCII);
        }

        internal static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private IActionResult TagNotFound()
        {
            HttpContext.Response.Headers.ETag = string.Format("\"{0}\"", Utils.Crypto.GetMD5(Utils.Password.CreatePassword(10)));
            var list = new List<string>();
            list.Add("#TAGNOTFOUND");
            list.Add("#OK");
            list.Add(Request.GetUri("/media/tagnotfound.mp3").ToString());
            return Content(string.Join("\r\n", list), "audio/x-mpegurl", Encoding.ASCII);
        }
    }
}