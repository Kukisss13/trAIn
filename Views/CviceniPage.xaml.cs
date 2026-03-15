using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

public partial class CviceniPage : ContentPage
{
    private readonly DatabaseService _db;
    private List<Cviceni> _vsechnyCviky = new();
    private string? _vybranaKategorie;

    public CviceniPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
        await NactiCviky();
    }

    private async Task NactiCviky()
    {
        _vsechnyCviky = await _db.GetCviceniAsync();

        var kategorie = _vsechnyCviky
            .Select(c => c.Kategorie)
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        VytvorKategorieTlacitka(kategorie);
        FiltrujCviky(null);

        PocetLabel.Text = string.Format(Loc.T("ExercisesTotal"), _vsechnyCviky.Count);
    }

    private void VytvorKategorieTlacitka(List<string> kategorie)
    {
        KategorieStack.Children.Clear();

        // Tlačítko "Vše"
        var btnVse = VytvorKategoriiButton(Loc.T("AllFilter"), true);
        btnVse.Clicked += (s, e) => FiltrujCviky(null);
        KategorieStack.Children.Add(btnVse);

        foreach (var kat in kategorie)
        {
            var ikona = KategorieIkona(kat);
            var btn = VytvorKategoriiButton($"{ikona} {Loc.CategoryName(kat)}", false);
            btn.Clicked += (s, e) => FiltrujCviky(kat);
            KategorieStack.Children.Add(btn);
        }
    }

    private Button VytvorKategoriiButton(string text, bool aktivni)
    {
        return new Button
        {
            Text = text,
            FontSize = 13,
            HeightRequest = 36,
            Padding = new Thickness(14, 0),
            CornerRadius = 18,
            BackgroundColor = aktivni ? Color.FromArgb("#7C4DFF") : Color.FromArgb("#1A1A2E"),
            TextColor = aktivni ? Colors.White : Color.FromArgb("#919191"),
            BorderColor = Colors.Transparent,
            FontAttributes = aktivni ? FontAttributes.Bold : FontAttributes.None,
        };
    }

    private static string KategorieIkona(string kat) => kat switch
    {
        "Hrudník" => "🏋️",
        "Záda"    => "🔙",
        "Nohy"    => "🦵",
        "Ramena"  => "🤸",
        "Ruce"    => "💪",
        "Core"    => "🔥",
        "Kardio"  => "❤️",
        _         => "⚡"
    };

    private void FiltrujCviky(string? kategorie)
    {
        _vybranaKategorie = kategorie;

        // Aktualizuj vzhled tlačítek
        foreach (var child in KategorieStack.Children)
        {
            if (child is Button btn)
            {
                bool aktivni = (kategorie == null && btn.Text == Loc.T("AllFilter")) ||
                               (kategorie != null && btn.Text == $"{KategorieIkona(kategorie)} {Loc.CategoryName(kategorie)}");
                btn.BackgroundColor = aktivni ? Color.FromArgb("#7C4DFF") : Color.FromArgb("#1A1A2E");
                btn.TextColor = aktivni ? Colors.White : Color.FromArgb("#919191");
                btn.FontAttributes = aktivni ? FontAttributes.Bold : FontAttributes.None;
            }
        }

        var filtrovane = kategorie == null
            ? _vsechnyCviky
            : _vsechnyCviky.Where(c => c.Kategorie == kategorie).ToList();

        CviceniList.ItemsSource = filtrovane;

        PocetLabel.Text = kategorie == null
            ? string.Format(Loc.T("ExercisesTotal"), _vsechnyCviky.Count)
            : string.Format(Loc.T("ExercisesInCategory"), filtrovane.Count, Loc.CategoryName(kategorie));
    }

    private async void OnPridatCvikClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        var nazev = await DisplayPromptAsync(Loc.T("NewExercise"), Loc.T("ExerciseName") + ":", Loc.T("Add"), Loc.T("Cancel"), Loc.T("ExerciseNamePlaceholder"));
        if (string.IsNullOrWhiteSpace(nazev))
            return;

        var kategorieKeys = new[] { "Hrudník", "Záda", "Nohy", "Ramena", "Ruce", "Core", "Kardio" };
        var kategorieNazvy = kategorieKeys.Select(k => Loc.CategoryName(k)).ToArray();
        var vyber = await DisplayActionSheet(Loc.T("SelectCategory"), Loc.T("Cancel"), null, kategorieNazvy);
        if (string.IsNullOrWhiteSpace(vyber) || vyber == Loc.T("Cancel"))
            return;
        var idx = Array.IndexOf(kategorieNazvy, vyber);
        var kategorie = idx >= 0 ? kategorieKeys[idx] : vyber;

        var popis = await DisplayPromptAsync(Loc.T("Description"), Loc.T("ShortDescOptional") + ":", Loc.T("OK"), Loc.T("Skip"), "");

        var novyCvik = new Cviceni
        {
            Nazev = nazev.Trim(),
            Kategorie = kategorie,
            Popis = popis?.Trim() ?? ""
        };

        await _db.UlozCviceniAsync(novyCvik);
        await NactiCviky();
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}
