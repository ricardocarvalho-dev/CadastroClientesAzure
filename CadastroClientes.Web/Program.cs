using CadastroClientes.Web.Components;
using CadastroClientes.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

// Configure logging para Azure
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.WebHost.CaptureStartupErrors(true).UseSetting("detailedErrors", "true");

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configura SQLite para Identity
string dataPath = Path.Combine(AppContext.BaseDirectory, "data");
if (string.IsNullOrEmpty(AppContext.BaseDirectory))
{
    dataPath = @"D:\home\site\data";
}
if (!Directory.Exists(dataPath))
    Directory.CreateDirectory(dataPath);

var dbPath = Path.Combine(dataPath, "cadastroclientes-web.db");

Batteries.Init();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// HttpClient para consumir a API
builder.Services.AddHttpClient("CadastroAPI", httpClient =>
{
    // Será sobrescrito em appsettings.json, mas aqui tem um padrão
    httpClient.BaseAddress = new Uri("http://localhost:5001");
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Cria/Migra o banco na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        Console.WriteLine(">>> Iniciando EF Core Migrate para Web App...");
        logger.LogInformation("Applying migrations to Web App...");
        db.Database.Migrate();
        Console.WriteLine(">>> EF Core Migrate concluído.");
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> Erro ao aplicar migrations: {ex.Message}");
        logger.LogError(ex, "Error applying migrations");
    }
}

app.Run();
