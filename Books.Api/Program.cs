using Books.Api.Contexts;
using Books.Api.Services;
using Books.Legacy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<BooksContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:BooksDBConnectionString"])); 

builder.Services.AddScoped<IBooksRepository, BooksRepository>();

builder.Services.AddTransient<ComplicatedPageCalculator>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
 