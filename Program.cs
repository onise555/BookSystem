using BookSystem.Data;
using BookSystem.Services;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "wwwroot"
});

// 1. სერვისების რეგისტრაცია
builder.Services.AddControllers()
    .AddFluentValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// მონაცემთა ბაზა
builder.Services.AddDbContext<DataContext>();

// გაძლიერებული Health Checks (ამოწმებს ბაზასაც)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DataContext>("Database");

builder.Services.AddScoped<S3Service>();

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

// 2. Middleware კონფიგურაცია

// გლობალური შეცდომების დამჭერი (Exception Handling)
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            // აქ შეგიძლია Serilog-ით ჩაწერო ლოგი: Log.Error($"Something went wrong: {contextFeature.Error}");
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error. Please try again later."
            });
        }
    });
});

// Swagger მხოლოდ დეველოპმენტისთვის (უსაფრთხოებისთვის)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookSystem API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors();
app.UseStaticFiles(); // სტანდარტული wwwroot

// ფაილების ატვირთვის ლოგიკა (Railway Volume vs Local)
var uploadRoot = Environment.GetEnvironmentVariable("UPLOAD_ROOT");
string finalUploadPath;

if (!string.IsNullOrWhiteSpace(uploadRoot))
{
    finalUploadPath = uploadRoot;
}
else
{
    finalUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
}

Directory.CreateDirectory(finalUploadPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(finalUploadPath),
    RequestPath = "/uploads"
});

app.UseAuthorization();

// ენდპოინტები
app.MapHealthChecks("/health");
app.MapControllers();

// 3. ავტომატური მიგრაცია (რომ ბაზა ყოველთვის განახლებული იყოს)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
}

// პორტის კონფიგურაცია Railway-სთვის
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");