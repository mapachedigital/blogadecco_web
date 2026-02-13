using BlogAdecco;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using MDMessaging;
using MDWidgets;
using MDWidgets.Utils;
using MDWidgets.Utils.ModelAttributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure the database context.  We can pass in the command line as an argument the database provider to use, e.g. dotnet ef migrations add NameOfMigration --project ../SqliteMigrations -- --DatabaseProvider Sqlite
// If using Sqlite then we use a different assembly (of a different project) for the migrations and it needs to be specified with the --project argument in the command line for the ef tool.
var sqlProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? (OperatingSystem.IsMacOS() ? "Sqlite" : "SqlServer");
switch (sqlProvider)
{
    case "SqlServer":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
                b =>
                {
                    b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));
        break;
    case "Sqlite":
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("SqliteConnection") ?? throw new InvalidOperationException("Connection string 'SqliteConnection' not found."),
                b =>
                {
                    b.MigrationsAssembly("SqliteMigrations");
                    b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));
        break;
    default:
        throw new InvalidOperationException($"Unsupported DatabaseProvider: {sqlProvider}");
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// For localization. Allows to inject the IStringLocalizer and IHtmlLocalizer service in the classes.  Use the Resources path for placing the resources
// in files named in the form of (example) Resources/Controllers.Api.ClientController.es.resx, the same for views: Resources/Views.Home.Index.es.resx
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Pretty descriptive options, right?
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    // Translate the error messages for passwords
    .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    // Add an additional check for user approval
    .AddSignInManager<ApprovedUserSignInManager<ApplicationUser>>();

// Add MVC Support
builder.Services.AddControllersWithViews()
    // For localization.  Allows to have localized Views (for example MyView.es.cshtml) and to inject the IViewLocalizer service in the view files.
    // call it by using `@inject IViewLocalizer` localizer in the top of your view and then you can use `localizer["My localized string"]`
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    // For localization.  Allows to have data annotation localized, for example for validation, using a IStringLocalizer
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(ModelResources));
    })
    .AddModelBindingMessagesLocalizer(builder.Services);

// Change to kebab-case routing
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.ConstraintMap["kebabcase"] = typeof(KebabCaseParameterTransformer);
});

// Change to kebab-case routing for Razor Pages
builder.Services.AddRazorPages(options => options.Conventions.Add(new KebabCasePageRouteModelConvention()));

// Service for Emailing: we have two versions, one needed for Identity and the other (nicer) for whatever you need.
builder.Services.AddMailUtils<IStringLocalizer<SharedResources>>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();

// Service for operation on user accounts
builder.Services.AddScoped<IUserUtils, UserUtils>();

// Service for storing files in Azure or locally
builder.Services.AddSingleton<IStorageUtils, StorageUtils>();

// Service for this site utilities
builder.Services.AddScoped<IBlogAdeccoUtils, BlogAdeccoUtils>();

// Service for processing shortcodes
builder.Services.AddSingleton<IShortcodeUtils, ShortcodeUtils>();

// Service for obtaining configuration settings
builder.Services.AddSingleton<IConfigUtils, ConfigUtils>();

// Service for site specific utilities
builder.Services.AddSiteUtils<IStringLocalizer<SharedResources>>();

// Service for model specific utilities
builder.Services.AddModelUtils<IStringLocalizer<ModelResources>>();

// Service for date and time specific utilities
builder.Services.AddDateTimeUtils<IStringLocalizer<SharedResources>>();

// Register HTTP client factory to be able to inject an IHttpClientFactory to make HTTP requests
builder.Services.AddHttpClient();

// This provider allows the translation of custom model attribute validators
builder.Services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();

// Add support for the messaging center
builder.Services.AddMDMessaging();

// Add cookies acceptance
builder.Services.AddCookiePolicy(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// As we're changing to kebab-case, we need to change the default path for Access Denied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/identity/account/access-denied";
    options.LoginPath = "/identity/account/login";
    options.LogoutPath = "/identity/account/logout";
});

// Configure supported cultures and localization options
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // State what the default culture for your application is. This will be used if no specific culture
    // can be determined for a given request.
    options.DefaultRequestCulture = new RequestCulture(builder.Configuration[MDGlobals.ConfigDefaultLanguage] ?? "es-MX");

    var supportedCulturesString = builder.Configuration.GetSection(MDGlobals.ConfigSupportedCultures).Get<List<string>>() ?? ["es-MX"];
    List<CultureInfo> supportedCultures = [.. supportedCulturesString.Select(x => new CultureInfo(x))];

    // Formatting numbers, dates, etc.
    options.SupportedCultures = supportedCultures;

    // UI strings that we have localized.
    options.SupportedUICultures = supportedCultures;
});

// Start the Azure emulator for development
var storageRemoteConnectionString = builder.Configuration[MDGlobals.ConfigStorageRemoteConnectionString];
if (storageRemoteConnectionString != null)
{
    builder.Services.AddAzureClients(options =>
    {
        options.AddBlobServiceClient(storageRemoteConnectionString, preferMsi: true).WithName("StorageConnection");
    });
}

var app = builder.Build();

// Enable to set the language by using the QueryString, Cookie and AcceptLanguage HTTP Header.
// In case you use a cookie, its name it should be .AspNetCore.culture and its format is c=%LANGCODE%|uic=%LANGCODE%
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseHttpsRedirection();

// Add cookies acceptance
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

app.MapControllerRoute(
      name: "areas",
      // Add the kebab case constraint to the area, controller and action
      pattern: "{area:kebabcase:exists}/{controller:kebabcase=Home}/{action:kebabcase=Index}/{id:kebabcase?}"
    )
    .WithStaticAssets();

// Tasks to perform once during App initialization
using (var scope = app.Services.CreateScope())
{
    // Instantiate the required services to be able to run the initialization
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var configUtils = services.GetRequiredService<IConfigUtils>();
    var userUtils = services.GetRequiredService<IUserUtils>();
    var mailUtils = services.GetRequiredService<IMailUtils>();
    var siteUtils = services.GetRequiredService<ISiteUtils>();

    //using var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Check that we have all the configuration strings required
        configUtils.CheckConfig();

        // Ensure the database is migrated
        //context.Database.Migrate();

        // Initialize superadmin
        await userUtils.InitializeRolesAsync();
    }
    catch (SqlException ex)
    {
        logger.LogError(ex, "Cannot initialize the database.");
        await mailUtils.SendMailAsync($"[{siteUtils.GetSiteName()}] Cannot initialize the database", ex.ToString(), "logs@mapachedigital.com");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Cannot start app");
        throw;
    }

    // Register the doppler form shortcodes for this blog
    // shortcodeUtils.Register("doppler-form", args => viewComponentRenderService.RenderToStringAsync("DopplerForm", new { id = int.Parse(args["id"]) }));


}

app.Run();
