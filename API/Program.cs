using API.Extensions;
using DataAccess.Executors;
using DataAccess.Models;
using DBHelper;
using Entities.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Services.CloudinaryService;
using Services.EmailService;
using Services.EventService;
using Services.FileLogger;
using Services.FollowService;
using Services.ImageUpload;
using Services.ProfileService;
using Services.UserAccount;

var builder = WebApplication.CreateBuilder(args);

var dbConnection = new DatabaseConnections();
builder.Configuration.Bind("Databases", dbConnection);

// Add services to the container.
builder.Services.AddSingleton(dbConnection);
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddScoped<IFileLogger, FileLogger>();

builder.Services.AddSingleton<IStoredProcedureExecutor, NpgsqlStoredProcedureExecutor>();
builder.Services.AddSingleton<IPostgresHelper, PostgresHelper>();
builder.Services.AddSingleton<IEmailSenderService, EmailSenderService>();
builder.Services.AddSingleton<IEventService, EventService>();
builder.Services.AddSingleton<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<ICloudinaryUploadService, CloudinaryUploadService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFollowingService, FollowingService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddCors();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});

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

app.UseSession();

app.UseMiddleware<BearerTokenDecoderMiddleware>();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000");
});


app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
