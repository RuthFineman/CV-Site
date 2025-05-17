using CVSite.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using CVSite.Core.Interfaces;
using CVSite.Service;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CVSite API", Version = "v1" });
});

builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection("GitHub"));

builder.Services.AddScoped<IGitHubService, GitHubService>();

builder.Services.AddMemoryCache();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CVSite API V1");
        options.RoutePrefix = "";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.Run();
