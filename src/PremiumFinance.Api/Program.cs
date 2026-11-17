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

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment() || app.Configuration["Swagger:Enabled"] == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Makes Program visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }