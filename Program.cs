using Dashboard.Extension;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using P2P.DbContextFolder;
using P2P.Model.User;
using static Dashboard.Helper.Utilities;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
var builder = WebApplication.CreateBuilder(args);



//Hangfire 
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("AuthConnection"))));

// Add services to the container.

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.ConfigureDbContext(builder.Configuration);

//Add policy
builder.Services.ConfigureRolePolicy();

builder.Services.AddMvc()
    .AddNewtonsoftJson(
        options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    );
builder.Services.AddHangfireServer();

builder.Services.AddCustomServices();

//Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opt =>
{
    opt.Password.RequiredLength = 1;
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireLowercase = false;
    opt.User.RequireUniqueEmail = false;
})
 .AddEntityFrameworkStores<AdminDbContext>()
 .AddDefaultTokenProviders();


builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Views/Forms/{1}/{0}" + RazorViewEngine.ViewExtension);
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});


builder.Services.ConfigureCookie(builder.Configuration);

builder.Services.AddSignalR();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseHangfireDashboard("/hangfire");

ServiceExtensions.RegisterRecurringJobs(app.Services.GetRequiredService<IConfiguration>());

app.UseRouting();
app.UseAuthentication();
// app.UseMiddleware<SecurityStampValidatorMiddleware>();

app.UseAuthorization();
// app.MapHub<NotyHub>("/RefreshNotification");
// app.MapHub<PayrollHub>("/payrollHub");
// app.MapHub<PRHub>("/prHub");



var fileServerConfig = builder.Configuration.GetSection("FileServer").Get<FileServerConfig>();
if (fileServerConfig == null || string.IsNullOrEmpty(fileServerConfig.Path))
{
    throw new InvalidOperationException("FileServer configuration or Path is missing.");
}
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(fileServerConfig.Path)),
    RequestPath = "/Files"
});

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Home", action = "Navigation" }
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
