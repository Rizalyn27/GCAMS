using GCAMS.Data;
using GCAMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<AppointmentStatusService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddControllersWithViews();

//Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//
OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("GCAMS Capstone Project");

// The host terminates SSL at its proxy, so the app receives a plain HTTP
// request even when the browser is on HTTPS. These headers carry the original
// scheme through, which is what makes UseHttpsRedirection and the secure
// cookie policy behave correctly once deployed.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Shared hosting doesn't give us a fixed proxy address to whitelist.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Auth cookies are encrypted with a key that lives in memory by default. The
// app pool recycles on idle, the key is lost, and everyone is silently signed
// out. Writing the keys to disk keeps sessions alive across restarts.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dp-keys")))
    .SetApplicationName("GCAMS");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        //For Cookies
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });



var app = builder.Build();

// Must come first — everything after it needs the corrected scheme.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var mustChange = context.User.FindFirst("PasswordChange")?.Value
            .Equals("false", StringComparison.OrdinalIgnoreCase) == true;

        var path = context.Request.Path.Value?.ToLower() ?? "";
        bool isExempt = path.StartsWith("/changepass")
             || path.StartsWith("/login")
             || path.StartsWith("/logout")
             || path.StartsWith("/css")
             || path.StartsWith("/js")
             || path.StartsWith("/lib");

        if (mustChange && !isExempt)
        {
            context.Response.Redirect("/ChangePass");
            return;
        }
    }

    await next();
});

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// Break-glass: make sure at least one administrator can always sign in.
// Runs in EVERY environment but does nothing unless there is no active admin.
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                      .CreateLogger("Bootstrap");

    SeedData.EnsureBootstrapAdmin(
        context,
        app.Configuration,
        logger,
        app.Environment.IsDevelopment());
}

app.Run();