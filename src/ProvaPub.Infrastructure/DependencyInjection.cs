using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProvaPub.Application.Commons.Persistences;
using ProvaPub.Infrastructure.Persistences.Database;

namespace ProvaPub.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder, string connectionName = "ctx")
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionName), "String de conexão não localizada.");

        builder.Services.AddDbContext<ProvaPubContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);

                sqlOptions.CommandTimeout(30);
            });
        });

        builder.Services.AddScoped<IProvaPubContext>(sp => sp.GetRequiredService<ProvaPubContext>());

        return builder;
    }
}