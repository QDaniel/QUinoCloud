using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace QUinoCloud.Pages.Manage.Uinos
{
    public class EditModel(AppDbContext context) : PageModel
    {
        [BindProperty]
        public UinoDevice Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id = 0)
        {
            if (id < 0) return NotFound();

            var entity = id != 0 ? await context.MyUinos(HttpContext).FirstOrDefaultAsync(m => m.Id == id) : new();
            if (entity == null) return NotFound();

            Entity = entity;

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var entity = Entity.Id != 0 ? await context.MyUinos(HttpContext).FirstOrDefaultAsync(m => m.Id == Entity.Id) : new();
            if (entity == null) return NotFound();

            entity.MAC = Entity.MAC;
            entity.Image = Entity.Image;
            entity.Title = Entity.Title;

            entity.SetOwner(HttpContext);
            if (entity.Id == 0) context.Add(entity);

            await context.SaveChangesAsync();


            return RedirectToPage("./Index");
        }
    }
}
