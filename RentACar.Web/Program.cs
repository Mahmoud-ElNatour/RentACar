using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;
using RentACar.Infrastructure.Data;
using RentACar.Infrastructure.Data.Repository;
using RentACar.Infrastructure.Repositories;
using AutoMapper;
using RentACar.Infrastructure.Data.Repositories;
using RentACar.Core.Managers;
using RentACar.Application.Managers;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RentACarDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()  // 🔥 Add this line to enable roles
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddRazorPages(); // 🔥 Add this line


//builder.Services.AddDefaultIdentity<AspNetUser>(options => {
//    options.SignIn.RequireConfirmedAccount = true;
//})
//.AddRoles<IdentityRole>()
//.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IBlacklistRepository, BlacklistRepository>();
builder.Services.AddScoped<IPromocodeRepository, PromocodeRepository>();
builder.Services.AddHttpContextAccessor();
// 🔥 Register Managers
builder.Services.AddScoped<CustomerManager>();
builder.Services.AddScoped<CategoryManager>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.AddScoped<EmployeeManager>();
builder.Services.AddScoped<CarManager>();
builder.Services.AddScoped<BlacklistManager>();
builder.Services.AddScoped<PromocodeManager>();
builder.Services.AddScoped<CreditCardManager>();





var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    // AutoMapper mapping validation (development only)
    var mapperConfig = new MapperConfiguration(cfg =>
    {
        cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
    });
    // mapperConfig.AssertConfigurationIsValid(); // mapping validation disabled

    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "de`fault",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
