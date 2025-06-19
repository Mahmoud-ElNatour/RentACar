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
using Serilog;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ✅ Set QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// ✅ Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// ✅ Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<RentACarDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();

// ✅ Register repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IBlacklistRepository, BlacklistRepository>();
builder.Services.AddScoped<IPromocodeRepository, PromocodeRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// ✅ Register managers
builder.Services.AddScoped<CustomerManager>();
builder.Services.AddScoped<CategoryManager>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.AddScoped<EmployeeManager>();
builder.Services.AddScoped<CarManager>();
builder.Services.AddScoped<BlacklistManager>();
builder.Services.AddScoped<PromocodeManager>();
builder.Services.AddScoped<CreditCardManager>();
builder.Services.AddScoped<PaymentMethodManager>();
builder.Services.AddScoped<BookingManager>();
builder.Services.AddScoped<PaymentManager>();

// ✅ HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7192;
});

var app = builder.Build();

// ✅ Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    var mapperConfig = new MapperConfiguration(cfg =>
    {
        cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
    });
    // mapperConfig.AssertConfigurationIsValid(); // Enable if needed
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
