using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using ImportWordpress.Data;
using ImportWordpress.Utils;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

/************
 * This project imports a Wordpress blog into .NET using our own defined classes
 ***********/

var builder = Host.CreateApplicationBuilder(args);

// We want debug info. This project is not intended to be run in a production environmnent
builder.Environment.EnvironmentName = "Development";

// We use the main project secrets.  For this, copy the <UserSecretsId> tag in the other project .csproj file into this projects file.
#pragma warning disable CS0436 // Type conflicts with imported type
builder.Configuration.AddUserSecrets<Program>();
#pragma warning restore CS0436 // Type conflicts with imported type

// This is the database context for the project we're importing from.  The connection string is defined in the secrets.json or appsettings.json of the main (the referenced) project
// under the section "ConnectionStrings".
builder.Services.AddDbContext<WordpressContext>(options => options.UseMySQL(
    builder.Configuration.GetConnectionString("WordpressConnection") ?? throw new InvalidOperationException("Connection string 'WordpressConnection' not found."),
    b =>
    {
        b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));

// This is the database context for the project we're importing to.  The connection string is defined in the secrets.json or appsettings.json of the main (the referenced) project
// under the section "ConnectionStrings".
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
        b =>
        {
            b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));

// We need Identity to create the super admin user, who will own the created entities
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Pretty descriptive options, right?
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IConfigUtils, ConfigUtils>();
builder.Services.AddScoped<IUserUtils, UserUtils>();
builder.Services.AddSingleton<IWebHostEnvironment, MyWebHostEnvironment>();
builder.Services.AddSingleton<IStorageUtils, StorageUtils>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var configUtils = services.GetRequiredService<IConfigUtils>();
var wpContext = services.GetRequiredService<WordpressContext>();
var dbContext = services.GetRequiredService<ApplicationDbContext>();
var userUtils = services.GetRequiredService<IUserUtils>();
var configuration = services.GetRequiredService<IConfiguration>();
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var userStore = services.GetRequiredService<IUserStore<ApplicationUser>>();
var storageUtils = services.GetRequiredService<IStorageUtils>();

// Start azurite local emulator in case it is needed
Process? azuriteProcess = null;
var fileLocation = Enum.Parse<FileLocation>(configuration[MDGlobals.ConfigStorageLocation] ?? throw new InvalidOperationException("Storage location config not found"));
var needsAzurite = fileLocation == FileLocation.Azure && configuration[MDGlobals.ConfigStorageRemoteConnectionString] == "UseDevelopmentStorage=true";
if (needsAzurite)
{
    var azuriteExecPath = @"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe";
    var azureDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @".vstools", "azurite");

    Console.WriteLine("Azure emulator location: " + azureDataPath);

    if (!File.Exists(azuriteExecPath))
    {
        throw new InvalidOperationException($"Azurite executable not found at {azuriteExecPath}");
    }

    var startInfo = new ProcessStartInfo
    {
        FileName = azuriteExecPath,
        Arguments = @$"--skipApiVersionCheck --location {azureDataPath}",
        UseShellExecute = true,
    };

    azuriteProcess = Process.Start(startInfo);
    Thread.Sleep(2000);
}

// Do the actual import
var wordpress = new ImportWordpress.ImportWordpress(
    blogContext: dbContext,
    wpContext: wpContext,
    configuration: configuration,
    userUtils: userUtils,
    userManager: userManager,
    userStore: userStore,
    configUtils: configUtils,
    storageUtils: storageUtils);

await wordpress.ImportAsync();

// Stop Azurite if needed
if (needsAzurite)
{
    Console.WriteLine("Process finished, you can now stop the Azurite emulator window");

    // Stop the azurite local emulator
    azuriteProcess?.WaitForExit();
}