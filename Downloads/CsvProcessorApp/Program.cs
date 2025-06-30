using CsvProcessorApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=appdata.db"));

// ✅ CORS (only once, and correct origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // important for cookies
    });
});

// ✅ Authentication with cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // These paths aren't used since Angular handles login
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAngularClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
