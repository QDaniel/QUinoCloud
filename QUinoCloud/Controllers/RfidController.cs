using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            if (tagInfo == null) return TagNotFound();
            context.CurrentUserID = tagInfo.OwnerId;
            var mac = Request.Headers["X-Ident"].ToString();
            mac = mac.ToUpperInvariant().Replace(":", "").Replace("-", "").Replace(" ", "").Trim();
            if (!string.IsNullOrWhiteSpace(mac) && mac.Length == 12)
            {
                var uino = await context.Devices.FirstOrDefaultAsync(o => o.MAC == mac);
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
            list.Add("#" + tagInfo.EditStamp);
            if (mode == Data.RfidTagMode.Cmd)
            {
                list.Add("#CMD:" + (char)Convert.ToInt32(tagInfo.Command!.Command));
            }


            if (mode == Data.RfidTagMode.Media)
            {
                incomplete = tagInfo.Media!.Duration != null;
                //list.Add(string.Format("#EXTINF:{0},{1}", tagInfo.Media!.Duration?.TotalSeconds, tagInfo.Media.DisplayTitle()));
                list.Add(tagInfo.Media.BuildUri(HttpContext).ToString());
            }
            if (mode == Data.RfidTagMode.Catalog && tagInfo.Catalog?.Medias != null)
            {
                foreach (var item in tagInfo.Catalog.Medias.OrderBy(o => o.Position).Select(o => o.Media))
                {
                    incomplete = incomplete | tagInfo.Media!.Duration != null;

                    //list.Add(string.Format("#EXTINF:{0},{1}", item.Duration?.TotalSeconds, item.DisplayTitle()));
                    list.Add(item!.BuildUri(HttpContext).ToString());
                }
            }
            list.Insert(1, incomplete ? "#INCOMPLETE" : "#OK");
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
            var list = new List<string>();
            list.Add("#TAGNOTFOUND");
            list.Add("#OK");
            list.Add(Request.GetUri("/media/tagnotfound.mp3").ToString());
            return Content(string.Join("\r\n", list), "audio/x-mpegurl", Encoding.ASCII);
        }
    }
}