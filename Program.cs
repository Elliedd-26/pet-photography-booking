using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register session and context accessor
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor(); 

// Optional: set up authentication scheme (recommended for better redirect control)
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.LoginPath = "/Login/Login"; // Fix wrong redirect to /Account/Login
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=PetPhotography.db"));

// Register Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed data here if needed

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Swagger is ON!");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();              // For session variables like UserRole
app.UseAuthentication();       // Required for custom login path to work
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
