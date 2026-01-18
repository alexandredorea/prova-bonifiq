using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProvaPub.Application.Commons.Behaviors;
using ProvaPub.Application.Commons.Middlewares;
using ProvaPub.Application.Commons.Patterns;

namespace ProvaPub.Application;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        builder.AddValidationsBusinessRule(assembly);
        builder.AddMediatorPattern(assembly);
        builder.AddStrategyPattern();
        builder.AddFactoryMethodPattern();
        return builder;
    }

    private static void AddValidationsBusinessRule(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.AddValidatorsFromAssembly(assembly);
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddSingleton<IDateTimeProvider, BrazilDateTimeProvider>();
    }

    private static void AddMediatorPattern(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    }

    private static void AddStrategyPattern(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPaymentStrategy, PixPaymentStrategy>();
        builder.Services.AddScoped<IPaymentStrategy, CreditCardPaymentStrategy>();
        builder.Services.AddScoped<IPaymentStrategy, PaypalPaymentStrategy>();
    }

    private static void AddFactoryMethodPattern(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPaymentFactory, PaymentFactory>();
    }

    public static IApplicationBuilder UseExceptionHandlingApplication(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}