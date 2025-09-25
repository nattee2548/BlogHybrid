using Amazon.S3;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Mappings;
using BlogHybrid.Application.Services;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Configuration;
using BlogHybrid.Infrastructure.Data;
using BlogHybrid.Infrastructure.Data.Seeds;
using BlogHybrid.Infrastructure.Repositories;
using BlogHybrid.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Cloudflare R2
builder.Services.Configure<CloudflareR2Options>(
    builder.Configuration.GetSection(CloudflareR2Options.SectionName));

// Register Amazon S3 client for Cloudflare R2
builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var r2Options = serviceProvider.GetRequiredService<IConfiguration>()
        .GetSection("CloudflareR2");

    // �� BasicAWSCredentials �¡
    var credentials = new Amazon.Runtime.BasicAWSCredentials(
        r2Options["AccessKeyId"],
        r2Options["SecretAccessKey"]
    );

    var config = new AmazonS3Config
    {
        ServiceURL = $"https://{r2Options["AccountId"]}.r2.cloudflarestorage.com",
        ForcePathStyle = true,
        UseHttp = false,
        AuthenticationRegion = r2Options["Region"] ?? "auto"
    };

    return new AmazonS3Client(credentials, config);
});

// Register Image Service
builder.Services.AddScoped<IImageService, CloudflareR2ImageService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ���� HttpContextAccessor ����Ѻ Image Service
builder.Services.AddHttpContextAccessor();

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Identity Options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(BlogHybrid.Application.Handlers.Auth.LoginUserHandler).Assembly));

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CategoryMappingProfile>();
});
// Repository Pattern & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(BlogHybrid.Application.Handlers.Auth.LoginUserHandler).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Admin", "Moderator"));
});

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DatabaseSeeder.SeedAllAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Admin Area Route (must come before default route)
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();