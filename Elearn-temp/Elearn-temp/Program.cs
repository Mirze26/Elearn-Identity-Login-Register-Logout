using Elearn_temp.Data;
using Elearn_temp.Models;
using Elearn_temp.Services;
using Elearn_temp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddSession(); 


builder.Services.AddDbContext<AppDbContext>(option =>
{

    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders(); 


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8; 
    options.Password.RequireDigit = true; 
    options.Password.RequireLowercase = true; 
    options.Password.RequireUppercase = true; 
    options.Password.RequireNonAlphanumeric = true;  
    options.User.RequireUniqueEmail = true; 

    options.Lockout.MaxFailedAccessAttempts = 3; 

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);  

    options.Lockout.AllowedForNewUsers = true; 

});




builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();  

builder.Services.AddScoped<ICourseService, CourseService>();  

builder.Services.AddScoped<IAuthorService, AuthorService>();  




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); 

app.UseSession(); 

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
