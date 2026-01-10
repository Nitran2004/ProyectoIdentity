using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using ProyectoIdentity.Servicios;
//using static ProyectoIdentity.Controllers.UsuariosController;

var builder = WebApplication.CreateBuilder(args);

// Configuración adicional para archivos de configuración
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Configuración de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

// Configuración de CORS
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Configuración de controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddIdentity<AppUsuario, IdentityRole>(options => {
    options.SignIn.RequireConfirmedEmail = true;

    // Mueve aquí tus configuraciones de password si las necesitas:
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 0;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 10;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<CustomIdentityErrorDescriber>(); // Si tenías uno personalizado

// Configuración de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Cuentas/Acceso");
    options.AccessDeniedPath = new PathString("/Cuentas/Denegado");
});

// Configuración de localización
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("es-ES") };
    options.DefaultRequestCulture = new RequestCulture("es-ES");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configuración de sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Servicios adicionales básicos
builder.Services.AddHttpContextAccessor();
// Registrar el servicio de Email para que el controlador pueda encontrarlo
builder.Services.AddTransient<IEmailSender, ServicioEmail>();
// DEJA SOLO ESTE Y AJUSTALO:

//// builder.Services.AddTransient<IEmailSender, MailJetEmailSender>();

// ========================================
// SERVICIOS PARA IA Y RECOMENDACIONES (SIN OLLAMA)
// ========================================


// Configuración de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

builder.Logging.SetMinimumLevel(LogLevel.Information);

// Construcción de la aplicación
var app = builder.Build();

// Configuración del pipeline de middleware
app.UseDeveloperExceptionPage(); // ⚠️ Temporal para ver el error real
app.UseHsts();                   // Mantén esto si estás en HTTPS



// Middleware
 app.UseHttpsRedirection();

// Configuración de archivos estáticos
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";

// Usa archivos estáticos con la configuración personalizada
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

// Middleware de CORS, sesión y autenticación
app.UseCors();
app.UseSession();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de rutas para APIs
app.MapControllers();

// Rutas específicas para el sistema de chat
app.MapControllerRoute(
    name: "chatApi",
    pattern: "api/chat/{action}",
    defaults: new { controller = "Home" });

app.MapControllerRoute(
    name: "recomendacionesApi",
    pattern: "api/recomendaciones/{action}",
    defaults: new { controller = "Recomendaciones" });

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear Razor Pages
app.MapRazorPages();

// ========================================
// INICIALIZACIÓN DEL SISTEMA DE RECOMENDACIONES (SIN OLLAMA)
// ========================================

app.Run();

// ========================================
// MÉTODOS AUXILIARES
// ========================================
