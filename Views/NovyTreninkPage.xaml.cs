using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

public partial class NovyTreninkPage : ContentPage
{
    private readonly DatabaseService _db;

    public NovyTreninkPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        DatumPicker.Date = DateTime.Today;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
    }

    private async void OnPokracovatClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        if (string.IsNullOrWhiteSpace(NazevEntry.Text))
        {
            await DisplayAlert(Loc.T("Error"), Loc.T("EnterTrainingName"), Loc.T("OK"));
            return;
        }

        // Hned ulož plán do databáze
        var plan = new TreninkovyPlan
        {
            Nazev = NazevEntry.Text.Trim(),
            Datum = DatumPicker.Date,
        };
        await _db.UlozPlanAsync(plan);

        // Naviguj s ID plánu
        await Shell.Current.GoToAsync($"trenink-tabulka?planId={plan.Id}");
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}
