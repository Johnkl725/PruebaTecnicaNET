using System.Text;
using Application;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var connection = configuration.GetConnectionString("UsersDatabase")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:UsersDatabase.");
        services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(connection));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<DevelopmentSeeder>();
        if (isDevelopment && configuration.GetValue<bool>("Seed:Enabled"))
            services.AddHostedService<DevelopmentSeedHostedService>();
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection("Jwt"))
            .Validate(x => Encoding.UTF8.GetByteCount(x.SigningKey) >= 32, "Jwt:SigningKey requiere al menos 32 bytes.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer) && !string.IsNullOrWhiteSpace(x.Audience), "Emisor y audiencia son obligatorios.")
            .Validate(x => x.DurationMinutes is >= 1 and <= 1440, "La duración debe estar entre 1 y 1440 minutos.")
            .ValidateOnStart();
        return services;
    }

}
