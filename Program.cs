using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HealthcareApi.Repositories;
using HealthcareApi.Services;
using Lucene.Net.Util;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

var AllowCORS = "_AllowCORS";

var builder = WebApplication.CreateBuilder(args);

// ————— Lucene Setup —————
const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;
var indexPath = Path.Combine(builder.Environment.ContentRootPath, "LuceneIndex");
var luceneDir = FSDirectory.Open(indexPath);
var analyzer = new StandardAnalyzer(LUCENE_VERSION);
builder.Services.AddSingleton(analyzer);
var indexConfig = new IndexWriterConfig(LUCENE_VERSION, analyzer) { OpenMode = OpenMode.CREATE_OR_APPEND };
var writer = new IndexWriter(luceneDir, indexConfig);
builder.Services.AddSingleton(writer);
builder.Services.AddSingleton<FSDirectory>(luceneDir);

// Register Lucene index services
builder.Services.AddScoped<LucenePatientIndexService>();
builder.Services.AddScoped<LuceneAppointmentIndexService>();
builder.Services.AddScoped<LuceneDoctorIndexService>();

// ————— Serilog —————
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ————— Session & MVC —————
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".HealthcareApi.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ————— EF Core —————
builder.Services.AddDbContext<AssignmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ————— Repositories & Services —————
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// ————— Auth & CORS —————
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["AuthConfiguration:Key"])),
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(opts => 
    opts.AddPolicy("RequireAdminRole", p => p.RequireRole("Admin")));

builder.Services.AddCors(opts =>
    opts.AddPolicy(AllowCORS, p => p
        .WithOrigins("http://localhost:4200", "http://localhost:5122")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    )
);

var app = builder.Build();

// ————— Seed Lucene Indexes —————
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    // Patients
    var patientRepo = sp.GetRequiredService<IPatientRepository>();
    var lucenePatient = sp.GetRequiredService<LucenePatientIndexService>();
    var allPatients = await patientRepo.GetBaseQuery()
        .ToListAsync();
    foreach (var p in allPatients)
        lucenePatient.IndexPatient(p);

    // Appointments
    var appointmentRepo = sp.GetRequiredService<IAppointmentRepository>();
    var luceneAppointment = sp.GetRequiredService<LuceneAppointmentIndexService>();
    var allAppointments = await appointmentRepo.GetBaseQuery()
        .Include(a => a.Patient)
        .Include(a => a.Doctor)
        .ToListAsync();
    foreach (var a in allAppointments)
        luceneAppointment.IndexAppointment(a);

    // Doctors
    var doctorRepo = sp.GetRequiredService<IDoctorRepository>();
    var luceneDoctor = sp.GetRequiredService<LuceneDoctorIndexService>();
    var allDoctors = await doctorRepo.GetBaseQuery()
        .ToListAsync();
    foreach (var d in allDoctors)
        luceneDoctor.IndexDoctor(d);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(AllowCORS);
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseSession();
app.MapControllers();
app.Run();
