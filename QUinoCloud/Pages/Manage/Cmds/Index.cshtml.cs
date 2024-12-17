using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;

namespace QUinoCloud.Pages.Manage.Cmds
{
    public class IndexModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public IList<CommandInfo> Entities { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Entities = await _context.MyCommands(HttpContext)
                .Include(r => r.Owner).ToListAsync();
        }
    }
}
