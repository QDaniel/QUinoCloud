using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Data;
using System;
using System.Security.Claims;
using System.Threading;
#nullable disable


public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public static string PublicUserID { get; set; }
    public string CurrentUserID { get; set; }
    public DbSet<MediaCatalogInfo> MediaCatalogs { get; set; }
    public DbSet<MediaInfo> MediaInfos { get; set; }
    public DbSet<MediaInfoRel> MediaInfoRels { get; set; }
    public DbSet<RfidTag> RfidCards { get; set; }
    public DbSet<CommandInfo> CommandInfos { get; set; }
    public DbSet<UinoDevice> Devices { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        SavingChanges += AppDbContext_SavingChanges;
    }

    private void AppDbContext_SavingChanges(object sender, SavingChangesEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserID)) return;
        foreach (var entityEntry in ChangeTracker.Entries<IOwnable>())
        {
            if (entityEntry.State == EntityState.Added || entityEntry.State == EntityState.Modified)
            {
                if (entityEntry.Entity is IEditStamp es) es.RenewEditStamp();
                if (string.IsNullOrWhiteSpace(entityEntry.Entity.OwnerId))
                    entityEntry.Entity.OwnerId = CurrentUserID;
                if (entityEntry.Entity.OwnerId != CurrentUserID)
                    throw new InvalidOperationException("Wrong User for Entity");
            }
            if (entityEntry.State == EntityState.Deleted)
            {
                if (entityEntry.Entity.OwnerId != CurrentUserID)
                    throw new InvalidOperationException("Wrong User for Entity");
            }
        }
    }

    internal bool AllowEdit(object obj)
    {
        if (obj is IOwnable oobj)
            return oobj.OwnerId == CurrentUserID;
        return true;
    }

    public IQueryable<T> My<T>(HttpContext ctx, IQueryable<T> dbset, bool showPublic) where T : IOwnable
    {
        Init(ctx);
        if (string.IsNullOrWhiteSpace(CurrentUserID)) throw new InvalidOperationException("not logged in");
        
        return showPublic ? dbset.Where(o => o.OwnerId == CurrentUserID || o.OwnerId == PublicUserID) : dbset.Where(o => o.OwnerId == CurrentUserID);
    }
    public IQueryable<MediaCatalogInfo> MyMediaCatalogs(HttpContext ctx, bool showPublic = false) => My(ctx, MediaCatalogs, showPublic);

    public IQueryable<MediaInfo> MyMedias(HttpContext ctx = null, bool showPublic = false) => My(ctx, MediaInfos, showPublic);

    public IQueryable<RfidTag> MyCards(HttpContext ctx = null, bool showPublic = false) => My(ctx, RfidCards, showPublic);
    
    public IQueryable<UinoDevice> MyUinos(HttpContext ctx = null, bool showPublic = false) => My(ctx, Devices, showPublic);

    public IQueryable<CommandInfo> MyCommands(HttpContext ctx = null, bool showPublic = false) => My(ctx, CommandInfos, showPublic);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public string MyMediaDir(HttpContext ctx = null, bool createit = true)
    {
        Init(ctx);
        if (string.IsNullOrWhiteSpace(CurrentUserID)) throw new InvalidOperationException("not logged in");
        var dir = Path.Combine("AppData", "Media", CurrentUserID);
        if (createit) Directory.CreateDirectory(dir);
        return dir;
    }

    internal void Init(HttpContext ctx)
    {
        if (ctx != null && string.IsNullOrWhiteSpace(CurrentUserID))
            CurrentUserID = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}

public static class DbExtensions
{
    public static void SetOwner(this IOwnable entry, HttpContext ctx)
    {
        var userid = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userid)) throw new InvalidOperationException("not logged in");
        if (string.IsNullOrWhiteSpace(entry.OwnerId))
        {
            entry.OwnerId = userid;
        }
        else
        {
            if (entry.OwnerId != userid) throw new InvalidOperationException("wrong user");
        }
    }
    public static Uri GetUri(this HttpRequest req, string relPath = null)
    {
        var urib = new UriBuilder(req.Scheme, req.Host.Host, req.Host.Port ?? -1)
        {
            Path = req.Path.Value ?? "/"
        };
        var uri = urib.Uri;
        if (!string.IsNullOrWhiteSpace(relPath)) uri = new Uri(uri, relPath);
        return uri;
    }
    public static Uri GetImageUri(this IImageable entry, HttpContext ctx)
    {
        Uri uri = null;
        if (!string.IsNullOrWhiteSpace(entry.Image))
        {
            Uri.TryCreate(ctx.Request.GetUri(), entry.Image.Contains("://") ? entry.Image : $"/dl/media/{entry.OwnerId}/{entry.Image}", out uri);
        }
        if (uri == null && entry is RfidTag tag) uri = tag.GetCmd().GetImageUri(ctx);
        return uri;
    }

}