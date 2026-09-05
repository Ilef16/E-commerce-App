using CommercialManagement.API.Interfaces;

namespace CommercialManagement.API.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProduitService, ProduitService>();
        services.AddScoped<ICommandeService, CommandeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
