using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.StaticFiles;
using PhotographerApp.Infrastructure.Data;
using PhotographerApp.Core.Validators;
using PhotographerApp.API.Middleware;
using FluentValidation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Local-only secrets (never committed — see appsettings.Local.json, gitignored). In production
// (e.g. Render) these same keys are set as real environment variables instead, which always take
// precedence since AddEnvironmentVariables() runs after this in ASP.NET Core's default config setup.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. Set it in appsettings.Local.json for local dev, " +
        "or as the ConnectionStrings__DefaultConnection environment variable in production.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecretKeyValue = jwtSettings["SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKeyValue))
    throw new InvalidOperationException(
        "JwtSettings:SecretKey not found. Set it in appsettings.Local.json for local dev, " +
        "or as the JwtSettings__SecretKey environment variable in production.");
var secretKey = Encoding.ASCII.GetBytes(jwtSecretKeyValue);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:3000",
            "https://localhost:5173",
            "https://localhost:3000",
            "https://both-dense-elevation.ngrok-free.dev",
            "http://both-dense-elevation.ngrok-free.dev"
        )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<PhotographerApp.Core.Interfaces.ITokenService, PhotographerApp.Infrastructure.Services.Auth.TokenService>();
// `dotnet publish` copies wwwroot next to the DLL; local `bin/Debug/net8.0` builds don't, so this
// falls back to reaching up to the project's own wwwroot for local dev only.
var publishedWwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
var wwwrootPath = Directory.Exists(publishedWwwroot)
    ? publishedWwwroot
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot");

builder.Services.AddScoped<PhotographerApp.Core.Interfaces.IFileStorageService>(provider =>
    new PhotographerApp.Infrastructure.Services.Business.FileStorageService(wwwrootPath));
builder.Services.AddScoped<PhotographerApp.Core.Interfaces.IContactService, PhotographerApp.Infrastructure.Services.Business.ContactService>();

// Film-led content services
builder.Services.AddScoped<PhotographerApp.Core.Interfaces.IFilmService, PhotographerApp.Infrastructure.Services.Content.FilmService>();

var app = builder.Build();

// Render (and similar platforms) assign the port to listen on via the PORT env var;
// launchSettings.json's URLs only apply to local `dotnet run`, not a published deployment.
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
    app.Urls.Add($"http://0.0.0.0:{renderPort}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactDev");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await PhotographerApp.Infrastructure.Data.Seeders.FilmSeeder.SeedAsync(dbContext);
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(wwwrootPath)),
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        context.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
