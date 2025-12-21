using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using ImportWordpress.Data;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
builder.Services.AddDbContext<BlogadeccoContext>(options => options.UseMySQL(
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

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var configUtils = services.GetRequiredService<IConfigUtils>();
var wpContext = services.GetRequiredService<BlogadeccoContext>();
var dbContext = services.GetRequiredService<ApplicationDbContext>();
var userUtils = services.GetRequiredService<IUserUtils>();
var configuration = services.GetRequiredService<IConfiguration>();

// Do the actual import
var wordpress = new ImportWordpress.ImportWordpress(dbContext, wpContext, configuration, userUtils, configUtils);
await wordpress.ImportAsync();