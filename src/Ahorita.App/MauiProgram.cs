using Ahorita.App.Features.Progress;
using Ahorita.App.Features.Routines;
using Ahorita.App.Features.Today;
using Ahorita.App.Services;
using Ahorita.Application;
using Ahorita.Infrastructure;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Ahorita.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Lexend-Regular.ttf", "LexendRegular");
                fonts.AddFont("Lexend-Medium.ttf", "LexendMedium");
                fonts.AddFont("Lexend-SemiBold.ttf", "LexendSemiBold");
                fonts.AddFont("Outfit-Bold.ttf", "OutfitBold");
                fonts.AddFont("Outfit-ExtraBold.ttf", "OutfitExtraBold");
            });

        builder.Services
            .AddApplication()
            .AddInfrastructure(Path.Combine(FileSystem.AppDataDirectory, "ahorita.db"))
            .AddPresentation();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<IToastService, ToastService>();

        services.AddTransient<TodayPage>().AddTransient<TodayViewModel>();
        services.AddTransient<RoutinesPage>().AddTransient<RoutinesViewModel>();
        services.AddTransient<ProgressPage>().AddTransient<ProgressViewModel>();

        return services;
    }
}
