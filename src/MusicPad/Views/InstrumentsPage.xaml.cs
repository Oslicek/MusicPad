using MusicPad.Services;

namespace MusicPad.Views;

/// <summary>
/// Page displaying the list of available instruments.
/// </summary>
public partial class InstrumentsPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private readonly List<InstrumentListItem> _items = new();

    public InstrumentsPage(ISfzService sfzService)
    {
        InitializeComponent();
        _sfzService = sfzService;
        
        LoadInstruments();
    }

    private void LoadInstruments()
    {
        _items.Clear();
        
        foreach (var name in _sfzService.AvailableInstruments)
        {
            var item = new InstrumentListItem
            {
                Name = name,
                Credit = ""
            };
            
            // Try to load instrument to get metadata (we'll cache this in future)
            // For now, just show the name
            _items.Add(item);
        }
        
        InstrumentsList.ItemsSource = _items;
    }

    private async void OnInstrumentSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is InstrumentListItem item)
        {
            // Clear selection so it can be selected again
            InstrumentsList.SelectedItem = null;
            
            // Navigate to instrument detail page with query parameter
            await Shell.Current.GoToAsync($"{nameof(InstrumentDetailPage)}?name={Uri.EscapeDataString(item.Name)}");
        }
    }
}

/// <summary>
/// View model for instrument list item.
/// </summary>
public class InstrumentListItem
{
    public string Name { get; set; } = "";
    public string Credit { get; set; } = "";
    public bool HasCredit => !string.IsNullOrEmpty(Credit);
}

