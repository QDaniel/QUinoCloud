﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Catalogs
{
    public class IndexModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public IList<MediaCatalogInfo> Entities { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Entities = await _context.MyMediaCatalogs(HttpContext)
                .Include(r => r.Owner).ToListAsync();
        }
    }
}
