using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Build.Evaluation;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CodeGenAPI", Version = "v1" });
    var filePath = Path.Combine(System.AppContext.BaseDirectory, "CodeGenAPI.xml");
    c.IncludeXmlComments(filePath);
    c.EnableAnnotations();
    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeGenAPI v1");
        c.InjectStylesheet("/swagger-custom.css");
    });
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


