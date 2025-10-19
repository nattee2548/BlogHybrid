using Amazon.Runtime;
using Amazon.S3;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Data;
using BlogHybrid.Infrastructure.Repositories;
using BlogHybrid.Infrastructure.Services;
using BlogHybrid.Web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// MediatR Configuration
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(BlogHybrid.Application.AssemblyReference).Assembly);
});

// FluentValidation Configuration
builder.Services.AddValidatorsFromAssembly(typeof(BlogHybrid.Application.AssemblyReference).Assembly);

// Repository & UnitOfWork Registration
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();

// Services Registration
builder.Services.AddScoped<BlogHybrid.Application.Interfaces.Services.ITagSimilarityService,
    BlogHybrid.Infrastructure.Services.TagSimilarityService>();

// AutoMapper Configuration (ถ้าไม่ใช้ให้เอาออกภายหลัง แต่ตอนนี้ต้องมีเพราะ handlers บางตัวใช้อยู่)
builder.Services.AddAutoMapper(cfg => { }, typeof(BlogHybrid.Application.AssemblyReference).Assembly);

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ISitemapService, SitemapService>();

// Register IImageService
builder.Services.AddScoped<IImageService, CloudflareR2ImageService>();

// Register AWS S3 Client for Cloudflare R2
builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
{
    var r2Options = serviceProvider.GetRequiredService<IConfiguration>()
        .GetSection("CloudflareR2");

    var credentials = new BasicAWSCredentials(
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllers();
// 1) Area routes ก่อน
app.MapAreaControllerRoute(
    name: "user_area",
    areaName: "User",
    pattern: "User/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

// 2) เส้นทางอื่น ๆ
app.MapControllerRoute(
    name: "create-community",
    pattern: "create-community",
    defaults: new { controller = "Community", action = "Create" });

app.MapControllerRoute(
    name: "community",
    pattern: "{categorySlug}/{communitySlug}",
    defaults: new { controller = "Community", action = "Details" },
    constraints: new
    {
        categorySlug = @"^(?!user|admin|account|home|api)([a-z0-9\-]+)$",
        communitySlug = @"^[a-z0-9\-]+$"
    });

// 3) Default route ปิดท้าย
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();