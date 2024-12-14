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
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public RfidTag Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id < 1) return NotFound();

            var entity = await _context.MyCards(HttpContext)
                .Include(o => o.Owner)
                .Include(o => o.Catalog)
                .Include(o => o.Command)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null) return NotFound();

            Entity = entity;
            return Page();
        }
    }
}
