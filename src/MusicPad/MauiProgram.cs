using MusicPad.Services;
using MusicPad.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MusicPad;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Montserrat-Regular.ttf", "Montserrat");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
            });

        // Configuration
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        // Options
        builder.Services.Configure<AudioSettings>(builder.Configuration.GetSection("Audio"));

        // Register services
        builder.Services.AddSingleton<ITonePlayer, TonePlayer>();
        builder.Services.AddSingleton<ISfzService, SfzService>();
        builder.Services.AddSingleton<IPadreaService, PadreaService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IInstrumentConfigService, InstrumentConfigService>();
        
        // Register pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<InstrumentsPage>();
        builder.Services.AddTransient<InstrumentDetailPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ImportInstrumentPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

