using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

[QueryProperty(nameof(Tab), "tab")]
public partial class TreninkyPage : ContentPage
{
    private readonly DatabaseService _db;
    private bool _jeAktivniTabPlany = false;

    public string Tab
    {
        set
        {
            if (value == "plany")
            {
                _jeAktivniTabPlany = true;
            }
        }
    }

    public TreninkyPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        DatumPicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
        await NactiData();

        if (_jeAktivniTabPlany)
        {
            OnTabPlanyClicked(null, EventArgs.Empty);
        }
    }

    private async Task NactiData()
    {
        var plany = await _db.GetPlanyAsync();
        var treninky = new List<TreninkZobrazeni>();
        var planZobrazeni = new List<TreninkZobrazeni>();

        foreach (var plan in plany)
        {
            var polozky = await _db.GetPolozkyPlanuAsync(plan.Id);
            var item = new TreninkZobrazeni
            {
                Id = plan.Id,
                Nazev = plan.Nazev,
                Datum = plan.Datum,
                PocetCviku = polozky.Count,
                JeKoncept = plan.JeKoncept,
            };

            planZobrazeni.Add(item);

            if (!plan.JeKoncept)
            {
                treninky.Add(item);
            }
        }

        TreninkyList.ItemsSource = treninky;
        PlanyList.ItemsSource = planZobrazeni;

        if (_jeAktivniTabPlany)
            PocetLabel.Text = string.Format(Loc.T("PlansCount"), planZobrazeni.Count);
        else
            PocetLabel.Text = string.Format(Loc.T("WorkoutsCount"), treninky.Count);
    }

    private void ResetTabs()
    {
        var inactive = Color.FromArgb("#888888");
        TabTreninky.BackgroundColor = Colors.Transparent;
        TabTreninkyLabel.TextColor = inactive;
        TabPlany.BackgroundColor = Colors.Transparent;
        TabPlanyLabel.TextColor = inactive;
        TabVytvorit.BackgroundColor = Colors.Transparent;
        TabVytvoritLabel.TextColor = inactive;

        TreninkyContent.IsVisible = false;
        PlanyContent.IsVisible = false;
        VytvoritContent.IsVisible = false;
    }

    private Color PrimaryColor => Color.FromArgb(((Color)Application.Current!.Resources["Primary"]).ToArgbHex());

    private async void OnTabTreninkyClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        _jeAktivniTabPlany = false;
        ResetTabs();
        TabTreninky.BackgroundColor = PrimaryColor;
        TabTreninkyLabel.TextColor = Colors.White;
        TreninkyContent.IsVisible = true;

        if (TreninkyList.ItemsSource is List<TreninkZobrazeni> list)
            PocetLabel.Text = string.Format(Loc.T("WorkoutsCount"), list.Count);
    }

    private async void OnTabPlanyClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        _jeAktivniTabPlany = true;
        ResetTabs();
        TabPlany.BackgroundColor = PrimaryColor;
        TabPlanyLabel.TextColor = Colors.White;
        PlanyContent.IsVisible = true;

        if (PlanyList.ItemsSource is List<TreninkZobrazeni> list)
            PocetLabel.Text = string.Format(Loc.T("PlansCount"), list.Count);
    }

    private async void OnTabVytvoritClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        _jeAktivniTabPlany = false;
        ResetTabs();
        TabVytvorit.BackgroundColor = PrimaryColor;
        TabVytvoritLabel.TextColor = Colors.White;
        VytvoritContent.IsVisible = true;
        PocetLabel.Text = Loc.T("NewPlan");
    }

    private async void OnTreninkClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        if (sender is Frame frame &&
            frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap &&
            tap.CommandParameter is TreninkZobrazeni trenink)
        {
            await Shell.Current.GoToAsync($"detailtreninku?planId={trenink.Id}");
        }
    }

    private async void OnPlanClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        if (sender is Frame frame &&
            frame.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap &&
            tap.CommandParameter is TreninkZobrazeni plan)
        {
            await Shell.Current.GoToAsync($"trenink-tabulka?planId={plan.Id}");
        }
    }

    private async void OnVytvoritPlanClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        if (string.IsNullOrWhiteSpace(NazevEntry.Text))
        {
            await DisplayAlert(Loc.T("Error"), Loc.T("EnterPlanName"), Loc.T("OK"));
            return;
        }

        var plan = new TreninkovyPlan
        {
            Nazev = NazevEntry.Text.Trim(),
            Datum = DatumPicker.Date,
        };
        await _db.UlozPlanAsync(plan);

        NazevEntry.Text = string.Empty;
        DatumPicker.Date = DateTime.Today;

        await Shell.Current.GoToAsync($"trenink-tabulka?planId={plan.Id}");
    }

    private async void OnSmazatPlanClicked(object? sender, EventArgs e)
    {
        if (sender is not View v) return;
        await AnimaceHelper.AnimovatKlik(v);

        var tap = (v as Frame)?.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
        if (tap?.CommandParameter is not TreninkZobrazeni plan) return;

        bool potvrdit = await DisplayAlert(Loc.T("DeletePlan"),
            string.Format(Loc.T("DeletePlanConfirm"), plan.Nazev), Loc.T("Delete"), Loc.T("Cancel"));
        if (!potvrdit) return;

        var planModel = await _db.GetPlanAsync(plan.Id);
        if (planModel != null)
        {
            await _db.SmazPolozkyPlanuAsync(plan.Id);
            await _db.SmazPlanAsync(planModel);
        }

        await NactiData();
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}

public class TreninkZobrazeni
{
    public int Id { get; set; }
    public string Nazev { get; set; } = string.Empty;
    public DateTime Datum { get; set; }
    public int PocetCviku { get; set; }
    public bool JeKoncept { get; set; }

    public string DenText => Datum.Day.ToString();
    public string MesicText => Loc.MonthAbbr(Datum.Month);
    public string PocetCvikuText => PocetCviku > 0 ? string.Format(Loc.T("ExercisesCount"), PocetCviku) : Loc.T("Empty");
    public string NazevSKonceptem => JeKoncept ? $"{Nazev}  •  {Loc.T("Draft")}" : Nazev;
    public Color KonceptBarva => JeKoncept ? Color.FromArgb("#E94560") : Colors.White;
}
