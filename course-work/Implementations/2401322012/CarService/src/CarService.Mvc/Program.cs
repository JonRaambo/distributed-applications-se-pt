using CarService.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(2);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

var apiBase = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:5001/";

builder.Services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBase);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
