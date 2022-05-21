using DataAccess.Executors;
using DataAccess.Models;
using DBHelper;
using Services.EventService;
using Services.FileLogger;

var builder = WebApplication.CreateBuilder(args);

var dbConnection = new DatabaseConnections();
builder.Configuration.Bind("Databases", dbConnection);

// Add services to the container.
builder.Services.AddSingleton(dbConnection);
builder.Services.AddScoped<IFileLogger, FileLogger>();
builder.Services.AddSingleton<IStoredProcedureExecutor, NpgsqlStoredProcedureExecutor>();
builder.Services.AddSingleton<IPostgresHelper, PostgresHelper>();
builder.Services.AddSingleton<IEventService, EventService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
