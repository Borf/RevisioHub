using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RevisioHub.Web.Components.Account;
using RevisioHub.Web.Components;
using RevisioHub.Web.Model.Db.User;
using RevisioHub.Web.Model.Db.Services;
using RevisioHub.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Net.Http.Headers;
using RevisioHub.Web.Model;
using Microsoft.Extensions.DependencyInjection;
using RevisioHub.Web.Controllers;
using System.Text.Json;

Configuration configuration = new();

var rawConfig = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();
rawConfig.Bind(configuration);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddSingleton<ServiceStatusService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "jwt_or_cookie";// IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = "jwt_or_cookie";// IdentityConstants.ExternalScheme;
        options.DefaultAuthenticateScheme = "jwt_or_cookie";// JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "jwt_or_cookie"; // JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "Authority URL"; // TODO: Update URL
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidAudience = configuration.JwtAudience,
            ValidIssuer = configuration.JwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JwtSecret))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = "";
                if (context.Request.Headers.Authorization.Count > 0 && context.Request.Headers.Authorization[0] != null)
                    accessToken = context.Request.Headers.Authorization[0];
                if (accessToken!.StartsWith("Bearer "))
                    accessToken = accessToken[(accessToken.IndexOf(" ") + 1)..];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/Client")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddPolicyScheme("jwt_or_cookie", "jwt_or_cookie", options =>
    {
        // runs on each request
        options.ForwardDefaultSelector = context =>
        {
            // filter by auth type
            string? authorization = context.Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                return JwtBearerDefaults.AuthenticationScheme;

            // otherwise always check for cookie auth
            return IdentityConstants.ApplicationScheme;
        };
    })
    .AddIdentityCookies();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=Database.db"));
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlite("Data Source=Database2.db"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
//    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
app.Services.CreateScope().ServiceProvider.GetRequiredService<Context>().Database.EnsureCreated();
app.MapHub<ClientService>("/Client");

app.UseStaticFiles(new StaticFileOptions()
{ 
   ServeUnknownFileTypes = true,
});
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
