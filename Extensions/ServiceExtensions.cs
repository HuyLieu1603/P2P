
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using P2P.DbContextFolder;

namespace Dashboard.Extension
{
    public static class ServiceExtensions
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            //services

        }
        //config dbcontext
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<AdminDbContext>(
              options =>
              {
                  options.UseNpgsql(configuration.GetConnectionString("AuthConnection"));
                  //   options.UseTriggers(x =>
                  //   {
                  //       x.AddTrigger<NotyTrigger>();
                  //   });
              }
            );
        }
        //cookie
        public static void ConfigureCookie(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = $"/Home/NotAccess";
                options.Cookie.Name = ".AspNet.SharedCookie";
                options.Cookie.Path = "/";
                options.Cookie.Domain = configuration.GetValue<string>("CookieDomain");
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);

                // options.Events.OnRedirectToAccessDenied = context =>
                // {
                //     var path = context.Request.Path.Value?.ToLower() ?? "";

                //     if (apiPrefixes.Any(prefix => path.StartsWith(prefix)))
                //     {
                //         context.Response.StatusCode = StatusCodes.Status403Forbidden;
                //         // context.Response.Redirect("/Home/NotAccess");
                //         return Task.CompletedTask;
                //     }

                //     context.Response.Redirect("/Home/NotAccess");
                //     return Task.CompletedTask;
                // };
            });

            // services.Configure<SecurityStampValidatorOptions>(options =>
            // {
            //     options.ValidationInterval = TimeSpan.FromMinutes(5);// refresh sau 5 phút
            // });

            //shared cookie
            var keysFolder = new DirectoryInfo(@"C:\NMV-HRM\keys");
            if (!keysFolder.Exists)
            {
                keysFolder.Create();
            }
            services.AddDataProtection()
                    .PersistKeysToFileSystem(keysFolder)
                    .SetApplicationName("NMVHRM");
        }

        //Config  file upload
        public static void ConfigureFileUpload(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100MB
            });
        }

        //Build hangfire jobs
        public static void RegisterRecurringJobs(IConfiguration configuration)
        {
            

        }


        //Role policy builder
        public static void ConfigureRolePolicy(this IServiceCollection services)
        {
         
        }
    }
}