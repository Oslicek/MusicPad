using MusicMap.Services;

namespace MusicMap;

public partial class MainPage : ContentPage
{
    private readonly ITonePlayer _tonePlayer;

    public MainPage(ITonePlayer tonePlayer)
    {
        InitializeComponent();
        _tonePlayer = tonePlayer;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StatusLabel.Text = "Ready";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tonePlayer.StopAll();
    }
}

