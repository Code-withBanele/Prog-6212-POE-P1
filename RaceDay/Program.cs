
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


var connectionString = builder.Configuration.GetConnectionString("RaceDayConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'RaceDayConnection' was not found.");
}

builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
