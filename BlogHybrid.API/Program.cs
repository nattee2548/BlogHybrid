//using Amazon.Runtime;
//using Amazon.S3;
//using BlogHybrid.API.Middleware;
//using BlogHybrid.Application.Interfaces.Repositories;
//using BlogHybrid.Application.Interfaces.Services;
//using BlogHybrid.Application.Mappings;
//using BlogHybrid.Application.Services;
//using BlogHybrid.Domain.Entities;
//using BlogHybrid.Infrastructure.Configuration;
//using BlogHybrid.Infrastructure.Data;
//using BlogHybrid.Infrastructure.Data.Seeds;
//using BlogHybrid.Infrastructure.Repositories;
//using BlogHybrid.Infrastructure.Services;
//using FluentValidation;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Serilog;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .CreateLogger();

//builder.Host.UseSerilog();

//// Add services to the container
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//// Configure Swagger with API Key
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "BlogHybrid API",
//        Version = "v1"
//    });

//    // API Key
//    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.ApiKey,
//        In = ParameterLocation.Header,
//        Name = "X-API-Key",
//        Description = "Enter your API Key"
//    });

//    // JWT Bearer
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        Description = "Enter your JWT token"
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "ApiKey"
//                }
//            },
//            Array.Empty<string>()
//        },
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});

//// CORS for Next.js
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("NextJsPolicy", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "https://yourdomain.com")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

//// Database Context
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
//    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(
//        connectionString,
//        b => b.MigrationsAssembly("BlogHybrid.Infrastructure")
//    ));

//// Identity
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    // Password settings
//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireNonAlphanumeric = true;
//    options.Password.RequiredLength = 8;

//    // User settings
//    options.User.RequireUniqueEmail = true;

//    // Lockout settings
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
//    options.Lockout.MaxFailedAccessAttempts = 5;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();

//// Configure Cloudflare R2
//builder.Services.Configure<CloudflareR2Options>(
//    builder.Configuration.GetSection(CloudflareR2Options.SectionName));

//// Register Amazon S3 client for Cloudflare R2
//builder.Services.AddSingleton<IAmazonS3>(serviceProvider =>
//{
//    var r2Options = serviceProvider.GetRequiredService<IConfiguration>()
//        .GetSection("CloudflareR2");

//    var credentials = new BasicAWSCredentials(
//        r2Options["AccessKeyId"],
//        r2Options["SecretAccessKey"]
//    );

//    var config = new AmazonS3Config
//    {
//        ServiceURL = $"https://{r2Options["AccountId"]}.r2.cloudflarestorage.com",
//        ForcePathStyle = true,
//        UseHttp = false,
//        AuthenticationRegion = r2Options["Region"] ?? "auto"
//    };

//    return new AmazonS3Client(credentials, config);
//});

//// Services
//builder.Services.AddScoped<IImageService, CloudflareR2ImageService>();
//builder.Services.AddScoped<ISlugService, SlugService>();

//// Repositories
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

//// MediatR
//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
//    typeof(BlogHybrid.Application.Handlers.Auth.LoginUserHandler).Assembly));

//// AutoMapper
//builder.Services.AddAutoMapper(cfg =>
//{
//    cfg.AddProfile<CategoryMappingProfile>();
//});

//// FluentValidation
//builder.Services.AddValidatorsFromAssembly(
//    typeof(BlogHybrid.Application.Handlers.Auth.LoginUserHandler).Assembly);

//// HttpContextAccessor
//builder.Services.AddHttpContextAccessor();

//// Authorization Policies
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Admin", "Moderator"));
//});

//// Configure JWT Settings
//builder.Services.Configure<JwtSettings>(
//    builder.Configuration.GetSection(JwtSettings.SectionName));

//var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

//// Add JWT Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.SaveToken = true;
//    options.RequireHttpsMetadata = false; // Set to true in production
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings.Issuer,
//        ValidAudience = jwtSettings.Audience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
//        ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
//    };
//});

//// Register Token Service
//builder.Services.AddScoped<ITokenService, TokenService>();

//var app = builder.Build();

//// Database Seeding
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var logger = services.GetRequiredService<ILogger<Program>>();

//    try
//    {
//        logger.LogInformation("Starting database seeding...");
//        await DatabaseSeeder.SeedAllAsync(services);
//        logger.LogInformation("Database seeding completed");
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "An error occurred while seeding the database");
//    }
//}

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogHybrid API v1");
//        options.DisplayRequestDuration();
//    });
//}
//else
//{
//    app.UseExceptionHandler("/error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseCors("NextJsPolicy");
//app.UseApiKeyAuth();  // API Key middleware
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//// Health check endpoint
//app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

//app.Run();

using Amazon.Runtime;
using Amazon.S3;
using BlogHybrid.API.Middleware;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using BlogHybrid.API.Filters;
using BlogHybrid.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with API Key
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BlogHybrid API",
        Version = "v1"
    });
    options.OperationFilter<FileUploadOperationFilter>();
    // API Key
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "Enter your API Key"
    });

    // JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {        
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    }
    else
    {        
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.WithOrigins(
                    "https://404talk.com",
                    "https://www.404talk.com"
                    //"https://admin.yourdomain.com"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
});

// Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        b => b.MigrationsAssembly("BlogHybrid.Infrastructure")
    ));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Cloudflare R2
builder.Services.Configure<CloudflareR2Options>(
    builder.Configuration.GetSection(CloudflareR2Options.SectionName));

builder.Services.Configure<CommunitySettings>(
    builder.Configuration.GetSection(CommunitySettings.SectionName));

// Register Amazon S3 client for Cloudflare R2
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

// Services
builder.Services.AddScoped<IImageService, CloudflareR2ImageService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<ITagSimilarityService, TagSimilarityService>();

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(BlogHybrid.Application.Handlers.Auth.LoginUserHandler).Assembly));

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<CategoryMappingProfile>();
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(
    typeof(BlogHybrid.Application.Handlers.Auth.LoginUserHandler).Assembly);

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Admin", "Moderator"));
});

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
    };
});

// Register Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting database seeding...");
        await DatabaseSeeder.SeedAllAsync(services);
        logger.LogInformation("Database seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogHybrid API v1");
        options.DisplayRequestDuration();
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");  
app.UseApiKeyAuth();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();