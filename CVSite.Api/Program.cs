//using CVSite.Service;
using CVSite.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using CVSite.Core.Interfaces;
using CVSite.Service;
var builder = WebApplication.CreateBuilder(args);

// ����������
builder.Services.AddControllers();

// CORS
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

// ����������� �� GitHubOptions ������ appsettings.json
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection("GitHub"));

// ����� �-DI �� ��������
builder.Services.AddScoped<IGitHubService, GitHubService>();
// �-Program.cs �� Startup.cs, ���� ����� ���:
builder.Services.AddMemoryCache();

var app = builder.Build();

// Swagger UI ���� �����
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
