using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

[QueryProperty(nameof(Sport), "sport")]
public partial class SportTimerPage : ContentPage
{
    private readonly DatabaseService _db;
    private string _sport = "beh";
    private double _vahaUzivatele = 70;
    private DateTime _start;
    private IDispatcherTimer? _timer;

    // GPS vzdálenost
    private static readonly HashSet<string> _sportySVzdalenosti = new() { "beh", "chuze", "kolo" };
    private CancellationTokenSource? _gpsCts;
    private Location? _posledniPoloha;
    private double _celkovaVzdalenostKm = 0;
    private bool _gpsPovoleno = false;

    public string Sport
    {
        set => _sport = string.IsNullOrEmpty(value) ? "beh" : value;
    }

    public SportTimerPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();

        // Načíst váhu uživatele
        var uzivatel = await _db.GetPrihlasenyUzivatelAsync();
        _vahaUzivatele = (uzivatel?.Vaha > 0) ? uzivatel.Vaha : 70;

        // Nastavit header
        string ikona = SportMet.GetIkona(_sport);
        string nazev = SportMet.GetNazev(_sport);
        SportIkonaNazevLabel.Text = $"{ikona}  {nazev}";

        // Nastavit start overlay
        StartSportIkona.Text = ikona;
        StartSportNazev.Text = nazev;

        // Info o výpočtu
        double met = SportMet.GetMet(_sport);
        InfoLabel.Text = string.Format(Loc.T("CalcInfo"), met, _vahaUzivatele);

        // GPS frame viditelnost nastavit předem
        if (_sportySVzdalenosti.Contains(_sport))
        {
            VzdalenostFrame.IsVisible = true;
            string jednotka = _sport == "kolo" ? "min/km" : "min/km";
            TempoJednotkaLabel.Text = jednotka;
        }
        else
        {
            VzdalenostFrame.IsVisible = false;
        }
    }

    private async void OnStartWorkoutClicked(object? sender, EventArgs e)
    {
        // Skrýt overlay, zobrazit ukončit button
        StartOverlay.IsVisible = false;
        UkoncitButton.IsVisible = true;
        HeaderSubtitleLabel.Text = Loc.T("WorkoutInProgress");

        // GPS pro sporty s měřením vzdálenosti
        if (_sportySVzdalenosti.Contains(_sport))
        {
            await SpustitGpsAsync();
        }

        // Spustit timer
        _start = DateTime.Now;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e2) =>
        {
            var elapsed = DateTime.Now - _start;
            StopkyLabel.Text = elapsed.Hours > 0
                ? elapsed.ToString(@"h\:mm\:ss")
                : elapsed.ToString(@"mm\:ss");
            KalorieLabel.Text = SportMet.SpocitejKalorie(_sport, _vahaUzivatele, elapsed).ToString();

            // Aktualizovat tempo pokud máme vzdálenost
            if (_gpsPovoleno && _celkovaVzdalenostKm > 0.05)
            {
                double tempoMinKm = elapsed.TotalMinutes / _celkovaVzdalenostKm;
                int tempoMin = (int)tempoMinKm;
                int tempoSek = (int)((tempoMinKm - tempoMin) * 60);
                TempoLabel.Text = $"{tempoMin}:{tempoSek:D2}";
            }
        };
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _gpsCts?.Cancel();
    }

    private async Task SpustitGpsAsync()
    {
        try
        {
            var stav = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (stav != PermissionStatus.Granted)
                stav = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (stav != PermissionStatus.Granted)
            {
                InfoLabel.Text += Loc.T("GpsUnavailable");
                return;
            }

            _gpsPovoleno = true;
            _gpsCts = new CancellationTokenSource();

            // Spustit GPS polling na pozadí
            _ = Task.Run(async () =>
            {
                while (!_gpsCts.Token.IsCancellationRequested)
                {
                    await AktualizovatPolohuAsync();
                    try
                    {
                        await Task.Delay(3000, _gpsCts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            });
        }
        catch
        {
            InfoLabel.Text += Loc.T("GpsError");
        }
    }

    private async Task AktualizovatPolohuAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(4));
            var nova = await Geolocation.GetLocationAsync(request);

            if (nova == null) return;

            // Filtrovat nepřesné lokace
            if (nova.Accuracy.HasValue && nova.Accuracy.Value > 40) return;

            if (_posledniPoloha != null)
            {
                double vzdalenostKm = Location.CalculateDistance(_posledniPoloha, nova, DistanceUnits.Kilometers);

                // Filtrovat GPS šum (min 3m, max 300m za 3 sekundy = 360 km/h)
                if (vzdalenostKm >= 0.003 && vzdalenostKm <= 0.3)
                {
                    _celkovaVzdalenostKm += vzdalenostKm;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        VzdalenostLabel.Text = _celkovaVzdalenostKm >= 1
                            ? _celkovaVzdalenostKm.ToString("F2")
                            : _celkovaVzdalenostKm.ToString("F3");
                    });
                }
            }

            _posledniPoloha = nova;
        }
        catch
        {
            // GPS chyba — tiše ignorovat, zkusíme příště
        }
    }

    private async void OnUkoncitClicked(object? sender, EventArgs e)
    {
        _timer?.Stop();
        _gpsCts?.Cancel();
        var elapsed = DateTime.Now - _start;
        int kalorie = SportMet.SpocitejKalorie(_sport, _vahaUzivatele, elapsed);

        var historie = new HistorieTreninku
        {
            TreninkovyPlanId = 0,
            NazevTreninku = SportMet.GetNazev(_sport),
            DatumCviceni = DateTime.Now,
            CasMinuty = Math.Round(elapsed.TotalMinutes, 1),
            CelkovaVahaKg = 0,
            CelkovaVzdalenostKm = Math.Round(_celkovaVzdalenostKm, 3),
            SpaleneKalorie = kalorie,
            TypSportu = _sport
        };
        await _db.UlozHistoriiAsync(historie);

        string casText = elapsed.Hours > 0
            ? elapsed.ToString(@"h\:mm\:ss")
            : elapsed.ToString(@"mm\:ss");

        string vzdalenostText = _gpsPovoleno && _celkovaVzdalenostKm > 0
            ? "\n" + string.Format(Loc.T("DistanceLabel"), _celkovaVzdalenostKm.ToString("F2"))
            : "";

        await DisplayAlert(Loc.T("WorkoutDone"),
            $"{string.Format(Loc.T("SportLabel"), Loc.SportName(_sport))}\n{string.Format(Loc.T("TimeLabel"), casText)}{vzdalenostText}\n{string.Format(Loc.T("BurnedLabel"), kalorie)}",
            Loc.T("OK"));

        await Shell.Current.GoToAsync("..");
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        bool opravdu = await DisplayAlert(Loc.T("EndWorkoutQuestion"),
            Loc.T("WorkoutNotSaved"), Loc.T("EndBtn"), Loc.T("ContinueBtn"));
        if (opravdu)
        {
            _timer?.Stop();
            _gpsCts?.Cancel();
            await Shell.Current.GoToAsync("..");
        }
    }
}
