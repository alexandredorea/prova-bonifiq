using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using ProvaPub.Application;
using ProvaPub.Infrastructure;

namespace ProvaPub.Api;

/// <summary>
///
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        var culture = CultureInfo.CreateSpecificCulture("pt-BR");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddControllers(option =>
        {
            option.RespectBrowserAcceptHeader = true;
            option.ReturnHttpNotAcceptable = true;
            option.AllowEmptyInputInBodyModelBinding = true;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services
            .Configure<RouteOptions>(option => { option.LowercaseUrls = true; })
            .Configure<ApiBehaviorOptions>(option => { option.SuppressModelStateInvalidFilter = true; }); //Suprime a validação automática do ModelState para que o FluentValidation seja o único responsável

        builder.Services.ConfigureHttpJsonOptions(option =>
        {
            option.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            option.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        builder.AddApplication();
        builder.AddInfrastructure();

        return builder;
    }
}