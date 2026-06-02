using System.Text;
using LIOSCare.DoctorDashboard.Infrastructure;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using LIOSCare.DoctorDashboard.Infrastructure.Security;
using LIOSCare.DoctorDashboard.Web.Security;
using LIOSCare.DoctorDashboard.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? string.Empty;
    if (jwtKey.Length < 32 || jwtKey.StartsWith("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException(
            "Jwt__SigningKey must be at least 32 characters and must not use the placeholder value. " +
            "Set it via the Jwt__SigningKey environment variable.");

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DoctorPortalDb")))
        throw new InvalidOperationException(
            "ConnectionStrings__DoctorPortalDb must be set in production. " +
            "Set it via the ConnectionStrings__DoctorPortalDb environment variable.");
}

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<PhotoStorageOptions>(builder.Configuration.GetSection("PhotoStorage"));
builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();
builder.Services.AddScoped<CurrentDoctorAccessor>();
builder.Services.AddScoped<LIOSCare.DoctorDashboard.Application.Contracts.ICurrentDoctorAccessor>(sp => sp.GetRequiredService<CurrentDoctorAccessor>());
builder.Services.AddDoctorDashboardInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = ".LIOSCare.DoctorDashboard.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/access-denied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromSeconds(30),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.Name
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("doctor"));
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ctx =>
    {
        var details = ctx.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
        return new BadRequestObjectResult(new { error = "Validation failed.", details, status = 400 });
    };
});
builder.Services.AddResponseCompression();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

if (app.Configuration.GetValue<bool>("Portal:AutoMigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DoctorPortalDbContext>();
    await db.Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("Portal:SeedDemoData"))
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DoctorDashboardSeeder>();
        await seeder.SeedAsync();
    }
}

app.UseMiddleware<LIOSCare.DoctorDashboard.Web.Middleware.AppExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseResponseCompression();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/auth/login"));
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.Run();
