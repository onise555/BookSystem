
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




// --- Middleware Pipeline (თანმიმდევრობა კრიტიკულია!) ---

// Swagger ყოველთვის ხელმისაწვდომი იქნება root-ზე (/)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookSystem API V1");
    c.RoutePrefix = string.Empty;
});

// უსაფრთხოება და წვდომა
app.UseHttpsRedirection();
app.UseCors(); // აუცილებლად MapControllers-მდე!
app.UseAuthorization();

// ენდფოინთები
app.MapControllers();

// Railway-სთვის პორტის მინიჭება
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");