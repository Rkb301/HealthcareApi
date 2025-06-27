using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HealthcareApi.Repositories;
using HealthcareApi.Services;

var AllowCORS = "_AllowCORS";

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".PowerHex.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.Expiration = TimeSpan.FromMinutes(1);
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AssignmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
#pragma warning disable CS8604 // Possible null reference argument.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["AuthConfiguration:Issuer"],
        ValidAudience = builder.Configuration["AuthConfiguration:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["AuthConfiguration:Key"]))
    };
#pragma warning restore CS8604 // Possible null reference argument.
});
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowCORS,
    policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5122")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();



// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

//app.UseExceptionHandler("/error");

// ordered middleware acc to best practices
app.UseHttpsRedirection();
app.UseCors(AllowCORS);
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging(); // Log all HTTP requests
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();
app.UseResponseCompression();

app.UseSession();

app.Run();
