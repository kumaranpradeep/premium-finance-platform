using PremiumFinance.Api.Middleware;
using PremiumFinance.Api.Services;
using PremiumFinance.Application.Common;
using PremiumFinance.Infrastructure;
using PremiumFinance.Application;
using PremiumFinance.Infrastructure.Persistence;
using PremiumFinance.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("PremiumFinanceDb")!);
builder.Services.AddApplication();
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
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

// Makes Program visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }