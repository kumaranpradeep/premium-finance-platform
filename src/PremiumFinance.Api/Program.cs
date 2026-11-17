using PremiumFinance.Api.Middleware;
using PremiumFinance.Api.Services;
using PremiumFinance.Application.Common;
using PremiumFinance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("PremiumFinanceDb")!);
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(cors => cors.AddPolicy("Portal", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));

var app = builder.Build();

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

app.Run();

// Makes Program visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }