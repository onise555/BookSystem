
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


// Swagger ყოველთვის ჩართული იქნება, მიუხედავად იმისა, დეველოპმენტში ხარ თუ არა
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechMarketplace API V1");
    c.RoutePrefix = string.Empty; // ეს ძალიან მნიშვნელოვანია! 404-ის ნაცვლად პირდაპირ Swagger-ს გახსნის
});

app.UseHttpsRedirection();
// დანარჩენი კოდი (Cors, Static Files, Auth და ა.შ.) დატოვე უცვლელი

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
