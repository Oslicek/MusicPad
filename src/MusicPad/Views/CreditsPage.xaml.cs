using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;
using MusicPad.Core.Theme;

namespace MusicPad.Views;

public partial class CreditsPage : ContentPage
{
    public CreditsPage()
    {
        InitializeComponent();
        ApplyTheme();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCreditsAsync();
    }
    
    private void ApplyTheme()
    {
        HeaderBar.BackgroundColor = Color.FromArgb(AppColors.Surface);
        BackArrow.TextColor = Color.FromArgb(AppColors.Accent);
        HeaderLabel.TextColor = Color.FromArgb(AppColors.TextPrimary);
        AppNameLabel.TextColor = Color.FromArgb(AppColors.TextPrimary);
        AppDescriptionLabel.TextColor = Color.FromArgb(AppColors.TextSecondary);
        CopyrightLabel.TextColor = Color.FromArgb(AppColors.TextMuted);
    }
    
    private async Task LoadCreditsAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("credits.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            
            var credits = JsonSerializer.Deserialize<CreditsData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (credits != null)
            {
                DisplayCredits(credits);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading credits: {ex.Message}");
            AppNameLabel.Text = "MusicPad";
            AppDescriptionLabel.Text = "Could not load credits information.";
        }
    }
    
    private void DisplayCredits(CreditsData credits)
    {
        AppNameLabel.Text = credits.AppName ?? "MusicPad";
        AppDescriptionLabel.Text = credits.AppDescription ?? "";
        CopyrightLabel.Text = credits.Copyright ?? "";
        
        SectionsContainer.Children.Clear();
        
        if (credits.Sections == null) return;
        
        foreach (var section in credits.Sections)
        {
            var sectionView = CreateSectionView(section);
            SectionsContainer.Children.Add(sectionView);
        }
    }
    
    private View CreateSectionView(CreditsSection section)
    {
        var container = new VerticalStackLayout { Spacing = 8 };
        
        // Section title
        var titleLabel = new Label
        {
            Text = section.Title ?? "",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(AppColors.Accent),
            Margin = new Thickness(0, 0, 0, 4)
        };
        container.Children.Add(titleLabel);
        
        if (section.Credits == null) return container;
        
        foreach (var credit in section.Credits)
        {
            var creditView = CreateCreditView(credit);
            container.Children.Add(creditView);
        }
        
        return container;
    }
    
    private View CreateCreditView(CreditItem credit)
    {
        var border = new Border
        {
            BackgroundColor = Color.FromArgb(AppColors.Surface),
            Stroke = Color.FromArgb(AppColors.BorderDark),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(12, 10)
        };
        
        var stack = new VerticalStackLayout { Spacing = 4 };
        
        // Name
        var nameLabel = new Label
        {
            Text = credit.Name ?? "",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(AppColors.TextPrimary)
        };
        stack.Children.Add(nameLabel);
        
        // Author (if present)
        if (!string.IsNullOrEmpty(credit.Author))
        {
            var authorLabel = new Label
            {
                Text = $"by {credit.Author}",
                FontSize = 12,
                TextColor = Color.FromArgb(AppColors.TextSecondary)
            };
            stack.Children.Add(authorLabel);
        }
        
        // Description (if present)
        if (!string.IsNullOrEmpty(credit.Description))
        {
            var descLabel = new Label
            {
                Text = credit.Description,
                FontSize = 12,
                TextColor = Color.FromArgb(AppColors.TextSecondary)
            };
            stack.Children.Add(descLabel);
        }
        
        // License
        if (!string.IsNullOrEmpty(credit.License))
        {
            var licenseLabel = new Label
            {
                Text = $"License: {credit.License}",
                FontSize = 11,
                TextColor = Color.FromArgb(AppColors.TextMuted)
            };
            stack.Children.Add(licenseLabel);
        }
        
        // URL (if present) - clickable
        if (!string.IsNullOrEmpty(credit.Url))
        {
            var urlLabel = new Label
            {
                Text = credit.Url,
                FontSize = 11,
                TextColor = Color.FromArgb(AppColors.LinkColor),
                TextDecorations = TextDecorations.Underline
            };
            
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                try
                {
                    await Launcher.OpenAsync(new Uri(credit.Url));
                }
                catch { }
            };
            urlLabel.GestureRecognizers.Add(tapGesture);
            
            stack.Children.Add(urlLabel);
        }
        
        // Notice (if present)
        if (!string.IsNullOrEmpty(credit.Notice))
        {
            var noticeLabel = new Label
            {
                Text = credit.Notice,
                FontSize = 10,
                TextColor = Color.FromArgb(AppColors.TextMuted),
                FontAttributes = FontAttributes.Italic,
                Margin = new Thickness(0, 4, 0, 0)
            };
            stack.Children.Add(noticeLabel);
        }
        
        border.Content = stack;
        return border;
    }
    
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

// JSON model classes
public class CreditsData
{
    public int Version { get; set; }
    public string? AppName { get; set; }
    public string? AppDescription { get; set; }
    public string? Copyright { get; set; }
    public List<CreditsSection>? Sections { get; set; }
}

public class CreditsSection
{
    public string? Title { get; set; }
    public List<CreditItem>? Credits { get; set; }
}

public class CreditItem
{
    public string? Name { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? License { get; set; }
    public string? Url { get; set; }
    public string? Notice { get; set; }
}



