using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopMemo.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopMemoCore(this IServiceCollection services)
    {
        services.AddSingleton<IDbContextFactory, DbContextFactory>();
        services.AddSingleton<IGroupService, GroupService>();
        services.AddSingleton<INoteService, NoteService>();
        services.AddSingleton<ITrashService, TrashService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        return services;
    }
}
