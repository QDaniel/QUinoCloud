﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace QUinoCloud.Pages.Manage.Uinos
{
    public class IndexModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public IList<UinoDevice> Entities { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Entities = await _context.MyUinos(HttpContext)
                .Include(r => r.Owner).ToListAsync();
        }
    }
}
