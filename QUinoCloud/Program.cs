using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NuGet.LibraryModel;
using QUinoCloud.Classes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

//    
//    .AddEntityFrameworkStores<AppDbContext>();
//builder.Services.AddIdentityCore<AppDbContext>();
// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<MediaDownloader>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

//.AddGoogle(googleOptions =>
//{
//    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
}); 
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
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


}

app.Run();