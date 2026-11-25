using PremiumFinance.Api.Middleware;
using PremiumFinance.Api.Services;
using PremiumFinance.Application.Common;
using PremiumFinance.Infrastructure;
using PremiumFinance.Application;
using PremiumFinance.Infrastructure.Persistence;
using PremiumFinance.Infrastructure.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adds the "Authorize" button: paste an access token once and Swagger sends it with every request.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken from POST /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("PremiumFinanceDb")!);
builder.Services.AddApplication();
// Authentication: who are you? Reads "Authorization: Bearer <token>" and checks the token.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep the claim names exactly as in the token ("sub", "role", "name"),
        // instead of renaming them to long Microsoft URIs.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),

            ValidateLifetime = true,
            // Default allows 5 minutes past expiry; with 15-minute tokens, keep it to 30 seconds.
            ClockSkew = TimeSpan.FromSeconds(30),

            RoleClaimType = "role",
            NameClaimType = "name"
        };
    });

// Authorization: what may you do? Fallback = any endpoint without its own rule requires a signed-in user.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
// /health: Healthy only if the API is running AND can connect to its database.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PremiumFinanceDbContext>("database");
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(cors => cors.AddPolicy("Portal", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));

var app = builder.Build();
// Create the first Admin if none exists (needs a scope: the seeder uses the DbContext).
using (IServiceScope scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync();
}

// Correlation ID first, so every log line after it (including errors) carries the ID.
app.UseMiddleware<CorrelationIdMiddleware>();
// Then exception handling, which catches errors from everything after it.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Configuration["Swagger:Enabled"] == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Portal");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Azure and the deploy pipeline call /health without a token.
app.MapHealthChecks("/health").AllowAnonymous();
app.Run();

// Makes Program visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }