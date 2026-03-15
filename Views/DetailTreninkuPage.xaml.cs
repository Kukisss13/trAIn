using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

[QueryProperty(nameof(PlanId), "planId")]
public partial class DetailTreninkuPage : ContentPage
{
    private readonly DatabaseService _db;
    private int _planId;
    private TreninkovyPlan? _plan;

    public string PlanId
    {
        set => _planId = int.TryParse(value, out var id) ? id : 0;
    }

    public DetailTreninkuPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
        await NactiDetail();
    }

    private async Task NactiDetail()
    {
        var plany = await _db.GetPlanyAsync();
        _plan = plany.FirstOrDefault(p => p.Id == _planId);
        if (_plan == null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        NazevLabel.Text = _plan.Nazev;
        DatumLabel.Text = _plan.Datum.ToString("d. MMMM yyyy");

        var polozky = await _db.GetPolozkyPlanuAsync(_planId);
        var zobrazeni = DetailPolozka.RozbalPolozky(polozky, _db);
        PolozkyList.ItemsSource = await zobrazeni;
    }

    private async void OnZacitTreninkClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync($"spustittrenink?planId={_planId}");
    }

    private async void OnSmazatClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        bool smazat = await DisplayAlert(Loc.T("DeleteTraining"),
            string.Format(Loc.T("DeleteTrainingConfirm"), _plan?.Nazev), Loc.T("Delete"), Loc.T("Cancel"));
        if (!smazat || _plan == null) return;

        // Smazat položky
        var polozky = await _db.GetPolozkyPlanuAsync(_planId);
        foreach (var p in polozky)
            await _db.SmazPolozkuAsync(p);

        // Smazat plán
        await _db.SmazPlanAsync(_plan);

        await Shell.Current.GoToAsync("..");
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}

public class DetailPolozka : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

    public string NazevCviku { get; set; } = string.Empty;
    public string SerieText { get; set; } = string.Empty;
    public string RepyVahaText { get; set; } = "-";
    public string PauzaText { get; set; } = "-";
    public bool JeHlavicka { get; set; }

    // Pauza v sekundach pro odpocet
    public int PauzaSekundy { get; set; }

    private bool _jeDokoncena;
    public bool JeDokoncena
    {
        get => _jeDokoncena;
        set { _jeDokoncena = value; OnPropertyChanged(nameof(JeDokoncena)); OnPropertyChanged(nameof(CheckTextColor)); OnPropertyChanged(nameof(RadekOpacity)); }
    }

    public Color CheckTextColor => JeDokoncena ? Colors.LimeGreen : Color.FromArgb("#555555");
    public double RadekOpacity => JeDokoncena ? 0.5 : 1.0;

    public static async Task<List<DetailPolozka>> RozbalPolozky(
        List<PolozkaPlanu> polozky, DatabaseService db)
    {
        var result = new List<DetailPolozka>();
        foreach (var p in polozky)
        {
            var nazev = !string.IsNullOrEmpty(p.NazevCviku) ? p.NazevCviku : null;
            if (nazev == null && p.CviceniId > 0)
            {
                var cvik = await db.GetCviceniByIdAsync(p.CviceniId);
                nazev = cvik?.Nazev;
            }
            nazev ??= Loc.T("UnknownExercise");

            var serie = !string.IsNullOrEmpty(p.OpakovaniDetail)
                ? p.OpakovaniDetail.Split(';', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>();

            int pocetSerii = serie.Length > 0 ? serie.Length : p.Serie;
            if (pocetSerii == 0) pocetSerii = 1;

            for (int i = 0; i < pocetSerii; i++)
            {
                string repyVaha = "-";
                if (i < serie.Length)
                {
                    var raw = serie[i].Trim();
                    var casti = raw.Split('/');
                    if (casti.Length >= 2)
                        repyVaha = $"{casti[0].Trim()}/{casti[1].Trim().Replace("kg", "")}kg";
                    else if (p.Vaha > 0)
                        repyVaha = $"{casti[0].Trim()}/{p.Vaha}kg";
                    else
                        repyVaha = casti[0].Trim();
                }

                result.Add(new DetailPolozka
                {
                    NazevCviku = i == 0 ? nazev : "",
                    SerieText = $"{i + 1}.",
                    RepyVahaText = repyVaha,
                    PauzaText = i == 0 && p.Pauza > 0 ? $"{p.Pauza}s" : "",
                    JeHlavicka = i == 0,
                    PauzaSekundy = p.Pauza,
                });
            }
        }
        return result;
    }
}
