using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Classes;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Catalogs
{
    public class EditModel(AppDbContext context) : PageModel
    {
        public string[] AllowedFileExt = [".mp3", ".flac", ".ogg"];

        [BindProperty]
        public int AddMediaId { get; set; } = default!;

        [BindProperty]
        public MediaCatalogInfo Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id = 0)
        {
            if (id < 0) return NotFound();

            var entity = id != 0 ? await context.MyMediaCatalogs(HttpContext).Include(o => o.Medias).FirstOrDefaultAsync(m => m.Id == id) : new();
            if (entity == null) return NotFound();

            Entity = entity;
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
            context.Init(HttpContext);

            var entity = Entity.Id != 0 ? await context.MyMediaCatalogs(HttpContext).FirstOrDefaultAsync(m => m.Id == Entity.Id) : new();
            if (entity == null) return NotFound();

            entity.Image = Entity.Image;
            entity.Title = Entity.Title;

            entity.SetOwner(HttpContext);
            if (entity.Id == 0) context.Add(entity);

            await context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
        public async Task<IActionResult> OnPostAddMediaAsync(int id)
        {
            context.Init(HttpContext);
            var entity = id != 0 ? await context.MyMediaCatalogs(HttpContext).Include(o => o.Medias).FirstOrDefaultAsync(m => m.Id == id) : null;

            if (entity?.Medias == null) return NotFound();

            var medias = context.MyMedias(HttpContext, true);
            if (AddMediaId <= 0) ModelState.AddModelError("AddMediaId", "Invalid value");
            else if ((await medias.FirstOrDefaultAsync(o => o.Id == AddMediaId)) is not MediaInfo mi) ModelState.AddModelError("AddMediaId", "No Permission for this Media");
            else
            {
                var mir = entity.Medias.FirstOrDefault(o => o.MediaId == mi.Id) ?? new();
                mir.MediaId = mi.Id;
                mir.CatalogId = entity.Id;
                mir.Position = int.MaxValue;
                if (mir.Id <= 0)
                {
                    context.MediaInfoRels.Add(mir);
                    entity.Medias.Add(mir);
                }
                var i = 0;
                foreach (var item in entity.Medias.OrderBy(o=>o.Position))
                {
                    item.Position = i++;
                }
                await context.SaveChangesAsync();
            }
            return RedirectToPage("./Edit", new { id });
        }
        public async Task<IActionResult> OnGetDelMediaRelAsync(int id)
        {
            context.Init(HttpContext);
            var rel = await context.MediaInfoRels.FindAsync(id);
            if (rel == null) return NotFound();
            var entity = await context.MyMediaCatalogs(HttpContext).FirstOrDefaultAsync(m => m.Id == rel.CatalogId);
            if (entity == null) return NotFound();

            context.MediaInfoRels.Remove(rel);
            await context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = entity.Id });
        }
    }
}
