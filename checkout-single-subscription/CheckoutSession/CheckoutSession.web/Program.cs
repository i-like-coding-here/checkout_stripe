using CheckoutSession.core.Configuration;
using CheckoutSession.core.Data;
using CheckoutSession.core.Interfaces;
using CheckoutSession.core.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Stripe;

Env.Load();

string staticDir = Environment.GetEnvironmentVariable("STATIC_DIR") ?? "wwwroot";
string staticDirFullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), staticDir));
if (!Directory.Exists(staticDirFullPath))
{
    throw new DirectoryNotFoundException($"STATIC_DIR path not found: {staticDirFullPath}");
}

string stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
    ?? throw new Exception("Missing STRIPE_SECRET_KEY");

StripeConfiguration.AppInfo = new AppInfo
{
    Name = "stripe-samples/checkout-single-subscription",
    Url = "https://github.com/stripe-samples/checkout-single-subscription",
    Version = "0.0.1",
};

StripeConfiguration.ApiKey = stripeSecretKey;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StripeOptions>(options =>
{
    options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");
    options.SecretKey = stripeSecretKey;
    options.WebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
    options.BasicPrice = Environment.GetEnvironmentVariable("BASIC_PRICE_ID");
    options.ProPrice = Environment.GetEnvironmentVariable("PRO_PRICE_ID");
    options.Domain = Environment.GetEnvironmentVariable("DOMAIN");
});

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<ICheckoutSessionService, CheckoutSessionService>();
builder.Services.AddDbContext<CheckoutSessionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("Missing connection string: DefaultConnection")));

builder.Services.AddDbContext<CheckoutSessionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using ( var scope = app.Services.CreateScope())
{
    var billingService = scope.ServiceProvider.GetRequiredService<ICheckoutSessionService>();
    await billingService.SyncPlansToStripe();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticDirFullPath),
    RequestPath = ""
});

app.UseCors("AllowLocalhost3000");

app.UseRouting();
app.MapGet("/", () => Results.Redirect("index.html"));
app.MapControllers();
app.Run();
