using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectOverview;
using ProjectOverview.Implementantion;
using ProjectOverview.Interface;
using ProjectOverview.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read Connection String
builder.Services.AddDbContext<ProjectOverviewContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:dbOverViewConnString",
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

var RedisConfig = builder.Configuration.GetSection("RedisConnString").Get<RedisConnString>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { $"{RedisConfig.Host}:{RedisConfig.Port}" },
    AbortOnConnectFail = false,
    AllowAdmin = true,
}));

builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddScoped<IMyProject, MyProject>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
