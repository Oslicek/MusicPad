using Microsoft.Maui.Controls.Shapes;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad.Views;

/// <summary>
/// Page for managing instruments with import, reorder, rename, and delete functionality.
/// </summary>
public partial class InstrumentsPage : ContentPage
{
    private readonly IInstrumentConfigService _configService;
    private readonly ISfzService _sfzService;
    private List<InstrumentConfig> _userInstruments = new();
    private List<InstrumentConfig> _bundledInstruments = new();
    private InstrumentConfig? _draggedItem;

    public InstrumentsPage(IInstrumentConfigService configService, ISfzService sfzService)
    {
        InitializeComponent();
        _configService = configService;
        _sfzService = sfzService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        RefreshPageColors();
        await LoadInstrumentsAsync();
    }
    
    private void RefreshPageColors()
    {
        // Get colors from current palette
        var bgPage = Color.FromArgb(AppColors.BackgroundPage);
        var surface = Color.FromArgb(AppColors.Surface);
        var textPrimary = Color.FromArgb(AppColors.TextPrimary);
        var textMuted = Color.FromArgb(AppColors.TextMuted);
        var secondary = Color.FromArgb(AppColors.Secondary);
        var textDark = Color.FromArgb(AppColors.TextDark);
        
        // Page background
        BackgroundColor = bgPage;
        
        // Header bar
        HeaderBar.BackgroundColor = surface;
        BackArrow.TextColor = textPrimary;
        HeaderLabel.TextColor = textPrimary;
        ImportButton.BackgroundColor = secondary;
        ImportButton.TextColor = textDark;
        
        // Section labels
        UserSectionLabel.TextColor = textMuted;
        BundledSectionLabel.TextColor = textMuted;
    }

    private async Task LoadInstrumentsAsync()
    {
        _userInstruments = await _configService.GetUserInstrumentsAsync();
        _bundledInstruments = await _configService.GetBundledInstrumentsAsync();
        
        BuildInstrumentLists();
    }

    private void BuildInstrumentLists()
    {
        // Clear existing
        UserInstrumentsList.Children.Clear();
        BundledInstrumentsList.Children.Clear();
        
        // Show/hide user section header
        UserSectionLabel.IsVisible = _userInstruments.Count > 0;
        
        // Build user instruments
        foreach (var config in _userInstruments)
        {
            var item = CreateInstrumentItem(config, isUser: true);
            UserInstrumentsList.Children.Add(item);
        }
        
        // Build bundled instruments
        foreach (var config in _bundledInstruments)
        {
            var item = CreateInstrumentItem(config, isUser: false);
            BundledInstrumentsList.Children.Add(item);
        }
    }

    private View CreateInstrumentItem(InstrumentConfig config, bool isUser)
    {
        var bgColor = isUser 
            ? Color.FromArgb(AppColors.SecondaryDark) // Amber for user
            : Color.FromArgb(AppColors.Surface);      // Teal for bundled
        
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = Color.FromArgb(AppColors.SurfaceBorder),
            BackgroundColor = bgColor,
            Padding = new Thickness(12, 10),
            BindingContext = config
        };
        
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto), // Drag handle
                new ColumnDefinition(GridLength.Star), // Name
                new ColumnDefinition(GridLength.Auto), // Buttons
            }
        };
        
        // Drag handle
        var dragHandle = new Label
        {
            Text = "≡",
            FontSize = 20,
            TextColor = Colors.White.WithAlpha(0.5f),
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        grid.Add(dragHandle, 0);
        
        // Name (tappable to view details)
        var nameLabel = new Label
        {
            Text = config.DisplayName,
            FontSize = 16,
            TextColor = Color.FromArgb(AppColors.TextPrimary),
            VerticalOptions = LayoutOptions.Center
        };
        
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) => await OnInstrumentTapped(config);
        nameLabel.GestureRecognizers.Add(tapGesture);
        grid.Add(nameLabel, 1);
        
        // Buttons container
        var buttonsStack = new HorizontalStackLayout { Spacing = 8 };
        
        if (isUser)
        {
            // Rename button
            var renameBtn = new Button
            {
                Text = "✎",
                FontSize = 14,
                BackgroundColor = Color.FromArgb(AppColors.Primary),
                TextColor = Colors.White,
                CornerRadius = 6,
                Padding = new Thickness(8, 4),
                WidthRequest = 36,
                HeightRequest = 36
            };
            renameBtn.Clicked += async (s, e) => await OnRenameClicked(config);
            buttonsStack.Add(renameBtn);
            
            // Delete button
            var deleteBtn = new Button
            {
                Text = "🗑",
                FontSize = 14,
                BackgroundColor = Color.FromArgb(AppColors.Accent),
                TextColor = Colors.White,
                CornerRadius = 6,
                Padding = new Thickness(8, 4),
                WidthRequest = 36,
                HeightRequest = 36
            };
            deleteBtn.Clicked += async (s, e) => await OnDeleteClicked(config);
            buttonsStack.Add(deleteBtn);
        }
        
        // Arrow for navigation
        var arrowLabel = new Label
        {
            Text = "›",
            FontSize = 24,
            TextColor = Color.FromArgb(AppColors.TextDim),
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        buttonsStack.Add(arrowLabel);
        
        grid.Add(buttonsStack, 2);
        
        border.Content = grid;
        
        // Add drag gesture for reordering
        var dragGesture = new DragGestureRecognizer { CanDrag = true };
        dragGesture.DragStarting += (s, e) => OnDragStarting(config, e);
        border.GestureRecognizers.Add(dragGesture);
        
        var dropGesture = new DropGestureRecognizer { AllowDrop = true };
        dropGesture.DragOver += OnDragOver;
        dropGesture.Drop += (s, e) => OnDrop(config, e);
        border.GestureRecognizers.Add(dropGesture);
        
        return border;
    }

    private async Task OnInstrumentTapped(InstrumentConfig config)
    {
        await Shell.Current.GoToAsync($"{nameof(InstrumentDetailPage)}?name={Uri.EscapeDataString(config.DisplayName)}");
    }

    private async Task OnRenameClicked(InstrumentConfig config)
    {
        var newName = await DisplayPromptAsync(
            "Rename Instrument",
            "Enter new name:",
            initialValue: config.DisplayName,
            maxLength: 50);
        
        if (!string.IsNullOrWhiteSpace(newName) && newName != config.DisplayName)
        {
            await _configService.RenameInstrumentAsync(config.FileName, newName);
            await LoadInstrumentsAsync();
        }
    }

    private async Task OnDeleteClicked(InstrumentConfig config)
    {
        var confirmed = await DisplayAlert(
            "Delete Instrument",
            $"Are you sure you want to delete '{config.DisplayName}'?\n\nThis cannot be undone.",
            "Delete",
            "Cancel");
        
        if (confirmed)
        {
            await _configService.DeleteInstrumentAsync(config.FileName);
            await LoadInstrumentsAsync();
        }
    }

    private void OnDragStarting(InstrumentConfig config, DragStartingEventArgs e)
    {
        _draggedItem = config;
        e.Data.Text = config.FileName;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void OnDrop(InstrumentConfig targetConfig, DropEventArgs e)
    {
        if (_draggedItem == null || _draggedItem.FileName == targetConfig.FileName)
        {
            _draggedItem = null;
            return;
        }
        
        // Reorder logic
        var allInstruments = new List<InstrumentConfig>();
        allInstruments.AddRange(_userInstruments);
        allInstruments.AddRange(_bundledInstruments);
        
        var draggedIndex = allInstruments.FindIndex(i => i.FileName == _draggedItem.FileName);
        var targetIndex = allInstruments.FindIndex(i => i.FileName == targetConfig.FileName);
        
        if (draggedIndex >= 0 && targetIndex >= 0)
        {
            var item = allInstruments[draggedIndex];
            allInstruments.RemoveAt(draggedIndex);
            allInstruments.Insert(targetIndex, item);
            
            // Save new order
            var orderedFileNames = allInstruments.Select(i => i.FileName).ToList();
            await _configService.SaveOrderAsync(orderedFileNames);
            await LoadInstrumentsAsync();
        }
        
        _draggedItem = null;
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ImportInstrumentPage));
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
