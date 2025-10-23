using AutoMapper;
using AwesomeNetwork;
using AwesomeNetwork.Data;
using AwesomeNetwork.Data.Repository;
using AwesomeNetwork.Data.UoW;
using AwesomeNetwork.Extentions;
using AwesomeNetwork.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var mapperConfig = new MapperConfiguration(v => v.AddProfile(new MappingProfile()));
IMapper mapper = mapperConfig.CreateMapper();

builder.Services.AddSingleton(mapper);

builder.Services
    .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection))
    .AddUnitOfWork()
    .AddCustomRepository<Message, MessageRepository>()
    .AddIdentity<User, IdentityRole>(opts => {
        opts.Password.RequiredLength = 5;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireLowercase = false;
        opts.Password.RequireUppercase = false;
        opts.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseDatabaseErrorPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var cachePeriod = app.Environment.IsDevelopment() ? "0" : "604800";
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();