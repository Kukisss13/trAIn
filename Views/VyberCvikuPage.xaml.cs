using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

public partial class VyberCvikuPage : ContentPage
{
    private readonly DatabaseService _db;
    private List<Cviceni> _vsechnyCviky = new();

    public VyberCvikuPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
        _vsechnyCviky = await _db.GetCviceniAsync();

        var kategorie = _vsechnyCviky
            .Select(c => c.Kategorie)
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        VytvorKategorieTlacitka(kategorie);
        FiltrujCviky(null);
    }

    private void VytvorKategorieTlacitka(List<string> kategorie)
    {
        KategorieStack.Children.Clear();

        var btnVse = VytvorButton("Vše", true);
        btnVse.Clicked += (s, e) => FiltrujCviky(null);
        KategorieStack.Children.Add(btnVse);

        foreach (var kat in kategorie)
        {
            var btn = VytvorButton(kat, false);
            btn.Clicked += (s, e) => FiltrujCviky(kat);
            KategorieStack.Children.Add(btn);
        }
    }

    private Button VytvorButton(string text, bool aktivni)
    {
        return new Button
        {
            Text = text,
            FontSize = 13,
            HeightRequest = 36,
            Padding = new Thickness(14, 0),
            CornerRadius = 18,
            BackgroundColor = aktivni ? Color.FromArgb("#FF6B35") : Color.FromArgb("#1A1A2E"),
            TextColor = aktivni ? Colors.White : Color.FromArgb("#919191"),
            BorderColor = Colors.Transparent,
            FontAttributes = aktivni ? FontAttributes.Bold : FontAttributes.None,
        };
    }

    private void FiltrujCviky(string? kategorie)
    {
        foreach (var child in KategorieStack.Children)
        {
            if (child is Button btn)
            {
                bool aktivni = (kategorie == null && btn.Text == "Vše") ||
                               (kategorie != null && btn.Text == kategorie);
                btn.BackgroundColor = aktivni ? Color.FromArgb("#FF6B35") : Color.FromArgb("#1A1A2E");
                btn.TextColor = aktivni ? Colors.White : Color.FromArgb("#919191");
                btn.FontAttributes = aktivni ? FontAttributes.Bold : FontAttributes.None;
            }
        }

        CviceniList.ItemsSource = kategorie == null
            ? _vsechnyCviky
            : _vsechnyCviky.Where(c => c.Kategorie == kategorie).ToList();
    }

    private async void OnCvikSelected(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        if (sender is Frame frame &&
            frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap &&
            tap.CommandParameter is Cviceni cvik)
        {
            MessagingCenter.Send(this, "CvikVybran", cvik);
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}
