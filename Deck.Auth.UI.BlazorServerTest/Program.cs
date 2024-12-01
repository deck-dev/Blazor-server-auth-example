using Deck.Auth.Application;
using Deck.Auth.Domain;
using Deck.Auth.UI.BlazorServerTest.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Deck.Auth.UI.BlazorServerTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();

            // Add custom services
            string connStr = builder.Configuration.GetConnectionString("UserCN")!;
            builder.Services.AddSingleton<IUserRepository, SqliteUserRepository>(provider =>
                ActivatorUtilities.CreateInstance<SqliteUserRepository>(provider, connStr));
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<MyAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<MyAuthStateProvider>());

            builder.Services.AddAuthentication("MyAuth")
                .AddCookie("MyAuth", options =>
                {
                    options.LoginPath = "/login"; // Redirect to login page
                    options.AccessDeniedPath = "/access-denied";
                });
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("UserPolicy", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || // Admin can bypass claims
                        context.User.HasClaim(c => c.Type == "Permission" && c.Value == "Banana")));

            var app = builder.Build();

            // Initialize services
            Initialize(app.Services);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        private static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var _ = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var authSvc = scope.ServiceProvider.GetRequiredService<IAuthService>();
            authSvc.RegisterUser("admin", "admin", "Admin").GetAwaiter().GetResult();
            authSvc.RegisterUser("deck", "1234").GetAwaiter().GetResult();

            // Add a claim to deck user
            var usr = authSvc.Authenticate("deck", "1234").GetAwaiter().GetResult();
            if (usr is not null)
            {
                authSvc.AddClaim(new UserClaim()
                {
                    UserId = usr.UserId,
                    ClaimName = "Banana"
                });
            }
        }
    }
}
