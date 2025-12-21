using MusicPad.Views;

namespace MusicPad;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(InstrumentsPage), typeof(InstrumentsPage));
        Routing.RegisterRoute(nameof(InstrumentDetailPage), typeof(InstrumentDetailPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(ImportInstrumentPage), typeof(ImportInstrumentPage));
        Routing.RegisterRoute(nameof(SongsPage), typeof(SongsPage));
        Routing.RegisterRoute(nameof(CreditsPage), typeof(CreditsPage));
    }
}

