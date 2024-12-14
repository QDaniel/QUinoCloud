using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Tags
{
    public class IndexModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public IList<RfidTag> Entities { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Entities = await _context.MyCards(HttpContext)
                .Include(r => r.Media)
                .Include(r => r.Command)
                .Include(r => r.Catalog)
                .Include(r => r.Owner).ToListAsync();
        }
    }
}
