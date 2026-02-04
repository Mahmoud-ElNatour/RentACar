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
using RentACar.Application.Managers;
using RentACar.Application.Services;
using RentACar.Web.Hubs;
using Serilog;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var fromConfig = builder.Configuration.GetConnectionString("DefaultConnection");
var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");



// ✅ Set QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// ✅ Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// ✅ Add services to the container
var connectionString = fromConfig ?? fromEnv;
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in config or environment.");
}

builder.Services.AddDbContext<RentACarDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        x => x.MigrationsHistoryTable("__ApplicationHistory", "dbo")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<RentACar.Web.Security.CustomUserClaimsPrincipalFactory>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3);
});

// ✅ Configure External Authentication (Google & Facebook)
// ✅ Configure External Authentication (Google & Facebook)
var authenticationBuilder = builder.Services.AddAuthentication();

var googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
var googleClientId = googleAuthNSection["ClientId"];
var googleClientSecret = googleAuthNSection["ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authenticationBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

var fbAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");
var fbAppId = fbAuthNSection["AppId"];
var fbAppSecret = fbAuthNSection["AppSecret"];

if (!string.IsNullOrEmpty(fbAppId) && !string.IsNullOrEmpty(fbAppSecret))
{
    authenticationBuilder.AddFacebook(options =>
    {
        options.AppId = fbAppId;
        options.AppSecret = fbAppSecret;
    });
}

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// ✅ Register repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IBlacklistRepository, BlacklistRepository>();
builder.Services.AddScoped<IPromocodeRepository, PromocodeRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
builder.Services.AddScoped<IEmailDraftRepository, EmailDraftRepository>();
builder.Services.AddHttpClient<IEmailService, MailjetEmailService>();
builder.Services.AddScoped<ICustomerRatingRepository, CustomerRatingRepository>();
builder.Services.AddScoped<ISupportConversationRepository, SupportConversationRepository>();
builder.Services.AddScoped<ISupportMessageRepository, SupportMessageRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverAvailabilityRepository, DriverAvailabilityRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<CustomerRatingManager>();
builder.Services.AddHttpClient<RentACar.Application.Services.IStripePaymentService, RentACar.Application.Services.StripePaymentService>(client =>
{
    client.BaseAddress = new Uri("https://api.stripe.com/");
});

// ✅ Register managers
builder.Services.AddScoped<CustomerManager>();
builder.Services.AddScoped<CategoryManager>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.AddScoped<EmployeeManager>();
builder.Services.AddScoped<DriverManager>();
builder.Services.AddScoped<TripManager>();
builder.Services.AddScoped<CarManager>();
builder.Services.AddScoped<BlacklistManager>();
builder.Services.AddScoped<PromocodeManager>();
builder.Services.AddScoped<PaymentMethodManager>();
builder.Services.AddScoped<BookingManager>();
builder.Services.AddScoped<PaymentManager>();
builder.Services.AddScoped<EmailManager>();
builder.Services.AddScoped<AuditLogManager>();
builder.Services.AddScoped<EmailTemplateManager>();
builder.Services.AddScoped<EmailDraftManager>();
builder.Services.AddScoped<DistributionListManager>();
builder.Services.AddScoped<RecipientResolverService>();
builder.Services.AddScoped<EmailProviderSettingsManager>();
builder.Services.AddScoped<SenderIdentityManager>();
builder.Services.AddScoped<EmailFeatureConfigManager>();
builder.Services.AddScoped<EmailRoutingService>();
builder.Services.AddScoped<NotificationProcessingService>();
builder.Services.AddScoped<EmailLogManager>();
builder.Services.AddScoped<SupportManager>();


// // ✅ HTTPS redirection
// builder.Services.AddHttpsRedirection(options =>
// {
//     options.HttpsPort = 7192;
// });

// ✅ Register Hosted Services
builder.Services.AddHostedService<RentACar.Web.Services.NotificationBackgroundService>();

var app = builder.Build();

// ✅ Automatically apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Identity/App Context (using __ApplicationHistory)
        try
        {
            var identityContext = services.GetRequiredService<ApplicationDbContext>();
            if (identityContext.Database.GetPendingMigrations().Any())
            {
                identityContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Identity Migration Skip: Database might already be up to date.");
        }

        // 2. Business Context (using __EFMigrationsHistory)
        try
        {
            var businessContext = services.GetRequiredService<RentACarDbContext>();
            if (businessContext.Database.GetPendingMigrations().Any())
            {
                businessContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Business Migration Skip: Database might already be up to date.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Critical error during migration sequence.");
    }
}

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

app.UseStatusCodePagesWithReExecute("/Home/NotFound");

app.UseRouting();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<SupportChatHub>("/supportChatHub");


app.MapHub<RentACar.Web.Hubs.DriverTrackingHub>("/hubs/driverTracking");
app.Run();
