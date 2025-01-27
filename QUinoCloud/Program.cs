using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using QUinoCloud.Classes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.Section));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});

//    
//    .AddEntityFrameworkStores<AppDbContext>();
//builder.Services.AddIdentityCore<AppDbContext>();
// Add services to the container.
builder.Services.AddResponseCaching();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<MediaDownloader>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddTransient<IEmailSender, EmailSender>();
//.AddGoogle(googleOptions =>
//{
//    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
});
builder.Services.AddControllersWithViews();
var app = builder.Build();

app.UseForwardedHeaders();
app.UseResponseCaching();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();


using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    var roleman = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!roleman.Roles.Any())
    {
        await roleman.CreateAsync(new IdentityRole("Admin"));
        await roleman.CreateAsync(new IdentityRole("User"));
        await roleman.CreateAsync(new IdentityRole("Child"));
    }

    var userman = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    if (!userman.Users.Any())
    {
        var pwd = QUinoCloud.Utils.Password.CreatePassword(10);
        await File.WriteAllTextAsync("AppData/adminpwd.txt", pwd);
        var user = new IdentityUser() { UserName = "admin", EmailConfirmed = true, Email = "admin@example.org" };
        var ret = await userman.CreateAsync(user, pwd);
        await userman.AddToRoleAsync(user, "Admin");
    }

    var publUser = context.Users.FirstOrDefault(o => o.UserName == "PUBLIC");
    if (publUser == null)
    {
        var pwd = QUinoCloud.Utils.Password.CreatePassword(10);
        await File.WriteAllTextAsync("AppData/publicpwd.txt", pwd);
        var user = new IdentityUser() { UserName = "PUBLIC", EmailConfirmed = true, Email = "no-reply@example.org" };
        var ret = await userman.CreateAsync(user, pwd);
    }
    AppDbContext.PublicUserID = publUser?.Id;
}

app.Run();