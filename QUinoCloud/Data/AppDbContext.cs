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
    public string CurrentUserID { get; set; }
    public DbSet<MediaCatalogInfo> MediaCatalogs { get; set; }
    public DbSet<MediaInfo> MediaInfos { get; set; }
    public DbSet<MediaInfoRel> MediaInfoRels { get; set; }
    public DbSet<RfidTag> RfidCards { get; set; }
    public DbSet<CommandInfo> CommandInfos { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        SavingChanges += AppDbContext_SavingChanges;
    }

    private void AppDbContext_SavingChanges(object sender, SavingChangesEventArgs e)
    {
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

    public IQueryable<T> My<T>(HttpContext ctx, IQueryable<T> dbset) where T : IOwnable
    {
        Init(ctx);
        if (string.IsNullOrWhiteSpace(CurrentUserID)) throw new InvalidOperationException("not logged in");
        return dbset.Where(o => o.OwnerId == CurrentUserID);
    }
    public IQueryable<MediaCatalogInfo> MyMediaCatalogs(HttpContext ctx) => My(ctx, MediaCatalogs);

    public IQueryable<MediaInfo> MyMedias(HttpContext ctx = null) => My(ctx, MediaInfos);

    public IQueryable<RfidTag> MyCards(HttpContext ctx = null) => My(ctx, RfidCards);

    public IQueryable<CommandInfo> MyCommands(HttpContext ctx = null) => My(ctx, CommandInfos);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public string MyMediaDir(HttpContext ctx = null, bool createit = true)
    {
        if (ctx != null) CurrentUserID = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(CurrentUserID)) throw new InvalidOperationException("not logged in");
        var dir = Path.Combine("AppData", "Media", CurrentUserID);
        if (createit) Directory.CreateDirectory(dir);
        return dir;
    }

    internal void Init(HttpContext ctx)
    {
        if (ctx != null)
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