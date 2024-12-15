using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using QUinoCloud.Utils.Extensions;
using System.Net;
using System.Text;

namespace QUinoCloud.Controllers
{
    [Route("api/device/")]
    public class DeviceController(AppDbContext context) : Controller
    {
        [AllowAnonymous]
        [Route("{mac}")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeviceUpdateAsync(string mac)
        {
            mac = mac.ToUpperInvariant().Replace(":", "").Replace("-", "").Replace(" ", "").Trim();
            var tagInfo = await context.Devices
                .FirstOrDefaultAsync(o => o.MAC == mac);

            if (tagInfo == null) return NotFound();
            var data = await Request.Body.ReadToStringAsync(Encoding.UTF8);

            tagInfo.LastSeen = DateTimeOffset.Now;
            if (!string.IsNullOrWhiteSpace(data)) tagInfo.LastInfo = data;
            await context.SaveChangesAsync();

            return NoContent();
        }

    }
}