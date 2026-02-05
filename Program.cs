
using BookSystem.Data;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();

builder.Services.AddControllers().AddFluentValidation(validation =>
{
    validation.AutomaticValidationEnabled = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors(build =>
{
    build.AddDefaultPolicy(config =>
    {
        config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

var app = builder.Build();



// Swagger-ის გამართვა (რომ პირდაპირ საიტის გახსნისას გამოჩნდეს)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookSystem API V1");
    c.RoutePrefix = string.Empty;
});

// Middleware-ების სწორი თანმიმდევრობა
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// პორტის დინამიური მიღება Railway-სგან
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");