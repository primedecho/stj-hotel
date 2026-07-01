using HotelSearch.Application.Hotels;
using HotelSearch.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IHotelSearchService, HotelSearchService>();

        return services;
    }
}
