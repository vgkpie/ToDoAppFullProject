using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// Register the DbContext with a connection string
builder.Services.AddDbContext<ToDoContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
    new MySqlServerVersion(new Version(8, 0, 21)))); // Adjust version as needed

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login"; // Redirect to login page
        options.LogoutPath = "/Home/Logout"; // Redirect to logout page
    });

var app = builder.Build();

// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ToDoContext>();

    // Remove existing admin user with invalid hash if any
    var existingAdmin = context.Users.FirstOrDefault(u => u.IsAdmin);
    if (existingAdmin != null)
    {
        context.Users.Remove(existingAdmin);
        context.SaveChanges();
    }

    // Create admin user with BCrypt hashed password
    var adminUser = new User
    {
        Username = "admin",
        Email = "admin@example.com",
        IsAdmin = true,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123") // Use BCrypt to hash password
    };
    context.Users.Add(adminUser);
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable authentication
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
