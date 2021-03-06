﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using Weapsy.Data;
using Weapsy.DependencyConfigurator;
using Weapsy.Mvc.Context;
using Weapsy.Mvc.ViewEngine;
using Weapsy.Models;
using Weapsy.Reporting.Pages;
using Weapsy.Services;
using System.Collections.Generic;
using Weapsy.Apps.Text;
using Weapsy.Core.Configuration;
using Weapsy.Reporting.Languages;
using Weapsy.Domain.Services.Installation;
using Weapsy.Domain.Model.Apps;
using Weapsy.Domain.Model.Sites;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Weapsy
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));

            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //services
            //    .AddScoped(p => new WeapsyDbContext(p.GetService<DbContextOptions<WeapsyDbContext>>()));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                //.AddViewComponentsAsServices
                //.AddControllersAsServices
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => options.ResourcesPath = "Resources")
                .AddDataAnnotationsLocalization()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new WeapsyViewLocationExpander());
                //options.FileProviders.Add(new CompositeFileProvider(new EmbeddedFileProvider(typeof(TextViewComponent).GetTypeInfo().Assembly, "Weapsy.Apps.Text")));
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IContextService, ContextService>();

            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new Domain.Data.SqlServer.AutoMapperProfile());
                cfg.AddProfile(new Reporting.Data.Default.AutoMapperProfile());
                cfg.AddProfile(new Apps.Text.Data.SqlServer.AutoMapperProfile());
            });

            services.AddSingleton(sp => autoMapperConfig.CreateMapper());

            var builder = new ContainerBuilder();

            builder.RegisterModule(new WeapsyModule());
            builder.RegisterModule(new AutofacModule());
            builder.Populate(services);

            var container = builder.Build();

            // ===== Temporary ===== //
            _appInstallationService = container.Resolve<IAppInstallationService>();
            _siteInstallationService = container.Resolve<ISiteInstallationService>();
            AppRepository = container.Resolve<IAppRepository>();
            SiteRepository = container.Resolve<ISiteRepository>();
            PageFacade = container.Resolve<IPageFacade>();
            LanguageFacade = container.Resolve<ILanguageFacade>();
            UserManager = container.Resolve<UserManager<ApplicationUser>>();
            RoleManager = container.Resolve<RoleManager<IdentityRole>>();
            // ==================== //

            return container.Resolve<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // ===== Temporary ===== //
            CheckIfDeafultAppsHaveBeenInstalled();
            CheckIfDeafultSiteHasBeenInstalled();
            CheckIfAdminUserHasBeenCreated();
            // ==================== //

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            app.UseStatusCodePagesWithRedirects("~/error/{0}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            var site = SiteRepository.GetByName("Default");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "area",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                
                if (site != null)
                {
                    foreach (var page in PageFacade.GetAllForAdminAsync(site.Id).Result)
                    {
                        var defaults = new RouteValueDictionary();
                        var constraints = new RouteValueDictionary();
                        var tokens = new RouteValueDictionary();

                        defaults.Add("controller", "Home");
                        defaults.Add("action", "Index");
                        defaults.Add("data", string.Empty);

                        constraints.Add("data", @"[a-zA-Z0-9\-]*");

                        tokens.Add("namespaces", new[] { "Weapsy.Controllers" });
                        tokens.Add("pageId", page.Id);

                        routes.MapRoute(
                            name: page.Name,
                            template: page.Url,
                            defaults: defaults,
                            constraints: constraints,
                            dataTokens: tokens);
                    }
                }

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //var supportedCultures = new[]
            //{
            //    new CultureInfo("en")
            //};

            //app.UseRequestLocalization(new RequestLocalizationOptions
            //{
            //    DefaultRequestCulture = new RequestCulture("en"),
            //    // Formatting numbers, dates, etc.
            //    SupportedCultures = supportedCultures,
            //    // UI strings that we have localized.
            //    SupportedUICultures = supportedCultures
            //});

            var supportedCultures = new List<CultureInfo>();
            RequestCulture defaultRequestCulture;

            try
            {
                foreach (var language in LanguageFacade.GetAllActive(site.Id))
                    supportedCultures.Add(new CultureInfo(language.CultureName));

                defaultRequestCulture = new RequestCulture(supportedCultures[0].Name);
            }
            catch (Exception)
            {
                supportedCultures.Add(new CultureInfo("en"));
                defaultRequestCulture = new RequestCulture("en");
            }

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = defaultRequestCulture,
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            //var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            //app.UseRequestLocalization(locOptions.Value);
        }

        private void CheckIfDeafultAppsHaveBeenInstalled()
        {
            if (AppRepository.GetByName("Text") == null)
            {
                _appInstallationService.InstallDefaultApps();
            }
        }

        private void CheckIfDeafultSiteHasBeenInstalled()
        {
            if (SiteRepository.GetByName("Default") == null)
            {
                _siteInstallationService.InstallDefaultSite();
            }
        }

        private async Task CheckIfAdminUserHasBeenCreated()
        {
            var adminEmail = "admin@default.com";
            var adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail };

            var adminRoleName = "Administrator";
            var adminRole = new IdentityRole(adminRoleName);

            if (await UserManager.FindByEmailAsync(adminEmail) == null)
            {
                await UserManager.CreateAsync(adminUser, "Ab1234567!");
            }

            if (!await RoleManager.RoleExistsAsync(adminRoleName))
            {
                await RoleManager.CreateAsync(adminRole);
            }

            adminUser = await UserManager.FindByEmailAsync(adminEmail);

            if (!await UserManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await UserManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }

        private static IAppInstallationService _appInstallationService;
        private static ISiteInstallationService _siteInstallationService;
        private static IAppRepository AppRepository { get; set; }
        private static ISiteRepository SiteRepository { get; set; }
        private static IPageFacade PageFacade { get; set; }
        private static ILanguageFacade LanguageFacade { get; set; }
        private static UserManager<ApplicationUser> UserManager { get; set; }
        private static RoleManager<IdentityRole> RoleManager { get; set; }
    }

    //public class ReferencePluginAssemblyProvider : DefaultAssemblyProvider
    //{
    //    //NOTE: The DefaultAssemblyProvider uses ILibraryManager to do the library/assembly querying
    //    public ReferencePluginAssemblyProvider(ILibraryManager libraryManager) : base(libraryManager)
    //    {
    //    }

    //    protected override HashSet<string> ReferenceAssemblies
    //        => new HashSet<string>(new[] { "Weapsy.Core" });
    //}
}
