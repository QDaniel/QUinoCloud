﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Cmds
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public CommandInfo Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id < 1) return NotFound();

            var entity = await _context.MyCommands(HttpContext)
                .Include(o => o.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null) return NotFound();

            Entity = entity;
            return Page();
        }
    }
}
