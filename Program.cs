using Microsoft.EntityFrameworkCore;
using abp_conference.Models;
using abp_conference.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ConferenceContext>(opt =>
    opt.UseInMemoryDatabase("ConferenceDB"));
builder.Services.AddMvc().AddControllersAsServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<ConferenceContext>();
    string defaultServices = "Проєктор: 500, Wi-Fi: 300, Звук: 700".ToLower();
    if (context != null)
    {

        context.Halls.AddRange([
            new Hall("Зал А", 50, defaultServices, 2000),
        new Hall("Зал B", 100, defaultServices, 3500),
        new Hall("Зал C", 30, defaultServices, 1500),
        ]);
        context.SaveChanges();
    }
}
    



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
