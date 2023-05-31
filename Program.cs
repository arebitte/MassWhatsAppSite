var builder = WebApplication.CreateBuilder(args);

// Configuração do ambiente
var environment = builder.Environment;
environment.EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

// Configuração do IP da API externa
builder.Configuration
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// Carregar configurações do arquivo appsettings.json



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddLogging();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
