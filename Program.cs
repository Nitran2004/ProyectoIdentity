using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using ProyectoIdentity.Servicios;

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


    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 0;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 10;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<CustomIdentityErrorDescriber>();

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
builder.Services.AddTransient<IEmailSender, ServicioEmail>();
builder.Services.AddHttpClient();

// Configuración de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ========================================
// CONSTRUCCIÓN DE LA APLICACIÓN (AQUÍ SE CREA 'app')
// ========================================
var app = builder.Build();

// ========================================
// ENDPOINTS DE MERCADO PAGO (AHORA SÍ PUEDES USAR 'app')
// ========================================
app.MapGet("/callback", (string? code, string? state) =>
{
    if (string.IsNullOrEmpty(code))
    {
        return Results.BadRequest("No se recibió código");
    }

    Console.WriteLine($"✅ Código recibido: {code}");
    Console.WriteLine($"State: {state}");
    return Results.Ok("¡Autorización exitosa! Puedes cerrar esta ventana.");
});

app.MapPost("/webhook", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    Console.WriteLine($"🔔 Notificación: {body}");
    return Results.Ok();
});

// ========================================
// CONFIGURACIÓN DEL PIPELINE
// ========================================
//app.UseDeveloperExceptionPage();
//app.UseHsts();

// AHORA (NET 8)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// Configuración de archivos estáticos
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";

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

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear Razor Pages
app.MapRazorPages();

app.Run();
