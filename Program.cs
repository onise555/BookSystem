using BookSystem.Data;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "wwwroot"
});

builder.Services.AddControllers()
    .AddFluentValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookSystem API V1");
    c.RoutePrefix = string.Empty;
});

app.UseCors();

// 1) wwwroot static files
app.UseStaticFiles();

// 2) uploads static files: PROD -> UPLOAD_ROOT, LOCAL -> wwwroot/uploads
var uploadRoot = Environment.GetEnvironmentVariable("UPLOAD_ROOT");

if (!string.IsNullOrWhiteSpace(uploadRoot))
{
    // PROD (Railway Volume): UPLOAD_ROOT=/data/uploads
    Directory.CreateDirectory(uploadRoot);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadRoot),
        RequestPath = "/uploads"
    });
}
else
{
    // LOCAL fallback: wwwroot/uploads
    var localUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    Directory.CreateDirectory(localUploads);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(localUploads),
        RequestPath = "/uploads"
    });
}

app.UseAuthorization();
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
