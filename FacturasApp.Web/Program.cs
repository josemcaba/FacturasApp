using FacturasApp.Core.Services;
using FacturasApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicio de Windows
builder.Host.UseWindowsService();

// Servicios
builder.Services.AddSingleton<ITextExtractor>(sp =>
    new WebTextExtractor(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata")));
builder.Services.AddSingleton<InvoiceProcessorService>();
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();

// Archivos estáticos (wwwroot/)
app.UseDefaultFiles();
app.UseStaticFiles();

// API controllers
app.MapControllers();

// Puerto de escucha
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");

app.Run();
