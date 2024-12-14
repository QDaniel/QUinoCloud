using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Tags
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public RfidTag Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id < 1) return NotFound();

            var entity = await _context.MyCards(HttpContext).FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null) return NotFound();

            Entity = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id < 1) return NotFound();

            var entity = await _context.MyCards(HttpContext).FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null) return NotFound();

            _context.RfidCards.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
