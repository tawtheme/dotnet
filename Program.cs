using BlueCollarEngine.API.AutoMapper;
using BlueCollarEngine.API.DBContext;
using BlueCollarEngine.API.Helpers;
using BlueCollarEngine.API.Middlewares;
using BlueCollarEngine.API.Repositories.LoggerRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using NLog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// this code for adding logger
LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// this code for adding response in camelcase
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// this code for implementing the automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// this code for adding dependencies 
builder.Services.AddScoped<ILoggerRepository, LoggerRepository>();

// this code for enabling CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlueCollarEngineAPI",
        builder => builder.WithOrigins("")
        .AllowAnyHeader()
        .AllowAnyOrigin());
});

// this code for validating the JWT token
builder.Services.AddTokenAuthentication(builder.Configuration);

// this code for adding connectionstring reference
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("conn")));

// this code for enabling dependency injection for IdentityRole and ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

// this code for adding JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

// this code for Swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "BlueCollarEngine API", Version = "V1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            } }, new string[] { }
    } });
});
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.ConfigureExceptionMiddleware(app.Environment, new LoggerRepository());
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint($"{builder.Configuration.GetSection("SwaggerConfig:ApplicationName").Value}/swagger/v1/swagger.json", "BlueCollarEngine API V1"));
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("BlueCollarEngineAPI");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.UseMiddleware<JwtMiddleware>();
app.Run();
