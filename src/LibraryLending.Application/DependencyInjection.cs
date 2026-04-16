using System.Reflection;
using FluentValidation;
using LibraryLending.Application.Common.Abstractions.Messaging;
using LibraryLending.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryLending.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AssemblyReference>(ServiceLifetime.Transient);
        services.AddHandlersFromAssembly(typeof(AssemblyReference).Assembly);
        services.AddTransient<LoanDomainService>();

        return services;
    }

    private static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var openHandlerTypes = new[]
        {
            typeof(ICommandHandler<,>),
            typeof(ICommandHandler<>),
            typeof(IQueryHandler<,>)
        };

        var implementations = assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false });

        foreach (var implementation in implementations)
        {
            var serviceTypes = implementation
                .GetInterfaces()
                .Where(@interface => @interface.IsGenericType)
                .Where(@interface => openHandlerTypes.Contains(@interface.GetGenericTypeDefinition()));

            foreach (var serviceType in serviceTypes)
            {
                services.AddTransient(serviceType, implementation);
            }
        }

        return services;
    }
}
