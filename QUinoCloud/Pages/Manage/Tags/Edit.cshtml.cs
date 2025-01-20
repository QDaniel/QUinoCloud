using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Tags
{
    public class EditModel(AppDbContext context) : PageModel
    {
        [BindProperty]
        public RfidTag Entity { get; set; } = default!;
        [BindProperty]
        public string Mode { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id = 0)
        {
            if (id < 0) return NotFound();

            var entity = id != 0 ? await context.MyCards(HttpContext).FirstOrDefaultAsync(m => m.Id == id) : new();
            if (entity == null) return NotFound();

            Entity = entity;

            ViewData["CommandId"] = new SelectList(context.MyCommands(HttpContext, true), "Id", "Title");
            ViewData["CatalogId"] = new SelectList(context.MyMediaCatalogs(HttpContext, true), "Id", "Title");
            ViewData["MediaId"] = new SelectList(context.MyMedias(HttpContext, true), "Id", "Title");
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

            var entity = Entity!.Id != 0 ? await context.MyCards(HttpContext).FirstOrDefaultAsync(m => m.Id == Entity.Id) : new();
            if (entity == null) return NotFound();

            entity.SerialNr = Entity.SerialNr;
            entity.Image = Entity.Image;
            entity.Title = Entity.Title;

            if (Mode == "mode_Media")
            {
                entity.Media = await context.MyMedias(HttpContext, true).FirstOrDefaultAsync(m => m.Id == Entity.MediaId);
                entity.CatalogId = null;
                entity.CommandId = null;
            }
            else if (Mode == "mode_Catalog")
            {
                entity.MediaId = null; 
                entity.Catalog = await context.MyMediaCatalogs(HttpContext, true).FirstOrDefaultAsync(m => m.Id == Entity.CatalogId);
                entity.CommandId = null;
            }
            else if (Mode == "mode_Cmd")
            {
                entity.MediaId = null;
                entity.CatalogId = null;
                entity.Command = await context.MyCommands(HttpContext, true).FirstOrDefaultAsync(m => m.Id == Entity.CommandId);
            }

            entity.SetOwner(HttpContext);
            entity.RenewEditStamp();
            if (entity.Id == 0) context.Add(entity);

            await context.SaveChangesAsync();


            return RedirectToPage("./Index");
        }
    }
}
