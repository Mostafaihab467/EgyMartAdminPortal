using EgyMartAdminPortal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EgyMartAdminPortal.Services;
using EgyMartAdminPortal.Handlers;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");

// Register signature handler
builder.Services.AddScoped<SignatureHandler>();
builder.Services.AddScoped<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient("PlainClient", client =>
{
    client.BaseAddress = new Uri(apiUrl!);
});

// Register HttpClient with handler
builder.Services.AddHttpClient("SignedClient", client =>
{
    client.BaseAddress = new Uri(apiUrl!);
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>()
.AddHttpMessageHandler<SignatureHandler>();

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("SignedClient");
});

// Services
builder.Services.AddScoped<SessionTimeoutService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<ToastrService>();
builder.Services.AddScoped<ImageService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<Func<AuthService>>(sp => () => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<HeaderMenuService>();
builder.Services.AddScoped<SliderMenuService>();
builder.Services.AddScoped<FooterMenuService>();

builder.Services.AddScoped<AttributeService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<FAQService>();
builder.Services.AddScoped<FixedPageService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<SocialMediaService>();
builder.Services.AddScoped<SubscribtionService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddRadzenComponents();


// Suppress HTTP client logging by setting LogLevel to Warning or higher in code
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

await builder.Build().RunAsync();
