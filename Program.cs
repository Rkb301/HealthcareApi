using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HealthcareApi.Repositories;
using HealthcareApi.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Lucene.Net.Util;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;

var AllowCORS = "_AllowCORS";

var builder = WebApplication.CreateBuilder(args);

const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;
var indexPath = Path.Combine(builder.Environment.ContentRootPath, "LuceneIndex");
var luceneDir = FSDirectory.Open(indexPath);

var analyzer = new StandardAnalyzer(LUCENE_VERSION);
builder.Services.AddSingleton<StandardAnalyzer>(analyzer);

var indexConfig = new IndexWriterConfig(LUCENE_VERSION, analyzer)
{
    OpenMode = OpenMode.CREATE_OR_APPEND
};
var writer = new IndexWriter(luceneDir, indexConfig);
builder.Services.AddSingleton<IndexWriter>(writer);

builder.Services.AddSingleton<FSDirectory>(luceneDir);

builder.Services.AddScoped<LucenePatientIndexService>();
builder.Services.AddScoped<LuceneAppointmentIndexService>();
builder.Services.AddScoped<LuceneDoctorIndexService>();

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
});
builder.Services.AddAuthorization(
    options =>
{
    options.AddPolicy("RequireAdminRole",
        policy => policy.RequireRole("Admin"));
}
);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowCORS,
    policy =>
    {
        policy.WithOrigins("http://localhost:4200","http://localhost:5122")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();

var lucenePatient = scope.ServiceProvider.GetRequiredService<LucenePatientIndexService>();
var patients = scope.ServiceProvider.GetRequiredService<IPatientService>().GetAllPatients().Result;
foreach (var p in patients) lucenePatient.IndexPatient(p);

var luceneAppointment = scope.ServiceProvider.GetRequiredService<LuceneAppointmentIndexService>();
var appointments = scope.ServiceProvider.GetRequiredService<IAppointmentService>().GetAllAppointments().Result;
foreach (var a in appointments) luceneAppointment.IndexAppointment(a);

var luceneDoctor = scope.ServiceProvider.GetRequiredService<LuceneDoctorIndexService>();
var doctors = scope.ServiceProvider.GetRequiredService<IDoctorService>().GetAllDoctors().Result;
foreach (var d in doctors) luceneDoctor.IndexDoctor(d);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseExceptionHandler("/error");

// ordered middleware acc to best practices
app.UseHttpsRedirection();
app.UseCors(AllowCORS);
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging(); // Log all HTTP requests
app.UseMiddleware<ErrorHandlerMiddleware>(); // custom exception handler
app.MapControllers();

app.UseSession();

app.Run();
