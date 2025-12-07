using ExamTwo.Controllers;
using ExamTwo.Data.Repositories;
using ExamTwo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar dependencias
builder.Services.AddSingleton<IDatabase>(sp =>
    new InMemoryDatabase(sp.GetRequiredService<ILogger<InMemoryDatabase>>()));
builder.Services.AddScoped<ICoffeeMachineService, CoffeeMachineService>();

// Configurar CORS para el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:8080",    
            "http://localhost:5173",    
            "http://localhost:3000",    
            "https://localhost:8080",
            "http://localhost:5000",
            "https://localhost:5001"
         )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configurar logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("VueFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
