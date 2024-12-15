using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QUinoCloud.Controllers
{
    [Route("dl/")]
    public class DownloadController(AppDbContext context) : Controller
    {
        [AllowAnonymous]
        [Route("media/{user}/{file}")]
        public async Task<IActionResult> DlFileAsync(string user, string file)
        {
            var path = Path.Combine("AppData", "Media", user, file);
            if (!System.IO.File.Exists(path)) return NotFound();

            var userData = await context.Users.FirstOrDefaultAsync(o => o.Id == user);

            if (userData == null) return NotFound();
            if (!userData.EmailConfirmed) return NotFound();
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = file,
                Inline = true
            };
            Response.Headers["Content-Disposition"] = cd.ToString();
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return File(System.IO.File.OpenRead(path), Utils.Mimetype.GetMimeTypeFromExtension(path) ?? "application/octet-stream", true);
        }
    }
}
