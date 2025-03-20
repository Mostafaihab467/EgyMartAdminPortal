using EgyMartAdminPortal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EgyMartAdminPortal.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration.GetValue<string>("ApiUrl");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl!) });

builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<ToastrService>();
builder.Services.AddScoped<ImageService>();

builder.Services.AddScoped<AuthService>();
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

await builder.Build().RunAsync();
