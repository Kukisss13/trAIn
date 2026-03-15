using Plugin.Maui.Audio;
using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

[QueryProperty(nameof(PlanId), "planId")]
[QueryProperty(nameof(Sport), "sport")]
public partial class SpustitTreninkPage : ContentPage
{
    private readonly DatabaseService _db;
    private int _planId;
    private string _sport = "posilovna";
    private double _vahaUzivatele = 70;
    private TreninkovyPlan? _plan;
    private List<PolozkaPlanu> _polozky = new();
    private DateTime _start;
    private IDispatcherTimer? _timer;
    private IDispatcherTimer? _pauzaTimer;
    private int _pauzaZbyva;
    private IAudioPlayer? _beepPlayer;
    private IAudioPlayer? _finishPlayer;

    public string PlanId
    {
        set => _planId = int.TryParse(value, out var id) ? id : 0;
    }

    public string Sport
    {
        set => _sport = string.IsNullOrEmpty(value) ? "posilovna" : value;
    }

    public SpustitTreninkPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        NactiZvuky();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();

        _plan = await _db.GetPlanByIdAsync(_planId);
        if (_plan == null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        // Načíst váhu uživatele pro výpočet kalorií
        var uzivatel = await _db.GetPrihlasenyUzivatelAsync();
        _vahaUzivatele = (uzivatel?.Vaha > 0) ? uzivatel.Vaha : 70;

        NazevLabel.Text = _plan.Nazev;
        _polozky = await _db.GetPolozkyPlanuAsync(_planId);

        var zobrazeni = await DetailPolozka.RozbalPolozky(_polozky, _db);
        PolozkyList.ItemsSource = zobrazeni;

        // Spustit stopky
        _start = DateTime.Now;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - _start;
            StopkyLabel.Text = elapsed.Hours > 0
                ? elapsed.ToString(@"h\:mm\:ss")
                : elapsed.ToString(@"mm\:ss");
            KalorieLabel.Text = SportMet.SpocitejKalorie(_sport, _vahaUzivatele, elapsed).ToString();
        };
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _pauzaTimer?.Stop();
    }

    private void OnSerieOdkliknuta(object? sender, EventArgs e)
    {
        if (sender is not View view) return;
        var polozka = view.BindingContext as DetailPolozka;
        if (polozka == null || polozka.JeDokoncena) return;

        polozka.JeDokoncena = true;

        // Spustit odpocet pauzy pokud ma cvik nastavenou pauzu
        if (polozka.PauzaSekundy > 0)
        {
            SpustitOdpocetPauzy(polozka.PauzaSekundy);
        }
    }

    private void SpustitOdpocetPauzy(int sekundy)
    {
        // Zastavit predchozi pauzu pokud bezi
        _pauzaTimer?.Stop();

        _pauzaZbyva = sekundy;
        AktualizujPauzaLabel();
        PauzaOverlay.IsVisible = true;

        _pauzaTimer = Dispatcher.CreateTimer();
        _pauzaTimer.Interval = TimeSpan.FromSeconds(1);
        _pauzaTimer.Tick += (s, e) =>
        {
            _pauzaZbyva--;
            if (_pauzaZbyva <= 0)
            {
                _pauzaTimer?.Stop();
                PauzaOverlay.IsVisible = false;
                PrehratFinish();
            }
            else
            {
                AktualizujPauzaLabel();
                // Pipnout posledních 10 sekund
                if (_pauzaZbyva <= 10)
                    PrehratBeep();
            }
        };
        _pauzaTimer.Start();
    }

    private async void NactiZvuky()
    {
        try
        {
            var audio = AudioManager.Current;
            var stream1 = await FileSystem.OpenAppPackageFileAsync("click.wav");
            _beepPlayer = audio.CreatePlayer(stream1);
            _beepPlayer.Volume = 0.6;

            var stream2 = await FileSystem.OpenAppPackageFileAsync("bell.wav");
            _finishPlayer = audio.CreatePlayer(stream2);
            _finishPlayer.Volume = 1.0;
        }
        catch { }
    }

    private void PrehratBeep()
    {
        try
        {
            if (_beepPlayer == null) return;
            if (_beepPlayer.IsPlaying) _beepPlayer.Stop();
            _beepPlayer.Seek(0);
            _beepPlayer.Play();
        }
        catch { }
    }

    private void PrehratFinish()
    {
        try
        {
            if (_finishPlayer == null) return;
            if (_finishPlayer.IsPlaying) _finishPlayer.Stop();
            _finishPlayer.Seek(0);
            _finishPlayer.Play();
        }
        catch { }
    }

    private void AktualizujPauzaLabel()
    {
        var ts = TimeSpan.FromSeconds(_pauzaZbyva);
        PauzaCountdownLabel.Text = ts.Minutes > 0
            ? $"{ts.Minutes}:{ts.Seconds:D2}"
            : $"0:{ts.Seconds:D2}";
    }

    private void OnPreskocitPauzuClicked(object? sender, EventArgs e)
    {
        _pauzaTimer?.Stop();
        PauzaOverlay.IsVisible = false;
    }

    private async void OnUkoncitClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        _timer?.Stop();

        var elapsed = DateTime.Now - _start;
        double celkovaVaha = SpocitejCelkovouVahu();
        int kalorie = SportMet.SpocitejKalorie(_sport, _vahaUzivatele, elapsed);

        var historie = new HistorieTreninku
        {
            TreninkovyPlanId = _planId,
            NazevTreninku = _plan?.Nazev ?? "",
            DatumCviceni = DateTime.Now,
            CasMinuty = Math.Round(elapsed.TotalMinutes, 1),
            CelkovaVahaKg = Math.Round(celkovaVaha, 1),
            CelkovaVzdalenostKm = 0,
            SpaleneKalorie = kalorie,
            TypSportu = _sport
        };
        await _db.UlozHistoriiAsync(historie);

        string casText = elapsed.Hours > 0
            ? elapsed.ToString(@"h\:mm\:ss")
            : elapsed.ToString(@"mm\:ss");

        await DisplayAlert(Loc.T("TrainingDone"),
            $"{string.Format(Loc.T("TimeDone"), casText)}\n{string.Format(Loc.T("LiftedDone"), historie.CelkovaVahaKg)}\n{string.Format(Loc.T("BurnedDone"), kalorie)}",
            Loc.T("OK"));

        await Shell.Current.GoToAsync("../..");
    }

    private double SpocitejCelkovouVahu()
    {
        double celkem = 0;
        foreach (var p in _polozky)
        {
            if (string.IsNullOrEmpty(p.OpakovaniDetail))
                continue;

            // Format: "12/80;10/85;8/90" (opakovani/vaha per serie, oddelene strednikem)
            // nebo "12,10,8" (jen opakovani bez vahy)
            var serie = p.OpakovaniDetail.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in serie)
            {
                var casti = s.Split('/');
                if (casti.Length >= 2)
                {
                    // opakovani/vaha
                    if (double.TryParse(casti[0].Trim(), out double opak) &&
                        double.TryParse(casti[1].Trim().Replace("kg", ""), out double vaha))
                    {
                        celkem += opak * vaha;
                    }
                }
                else if (p.Vaha > 0)
                {
                    // jen opakovani, vaha je v poli Vaha
                    if (double.TryParse(casti[0].Trim(), out double opak))
                    {
                        celkem += opak * p.Vaha;
                    }
                }
            }
        }
        return celkem;
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        bool opravdu = await DisplayAlert(Loc.T("EndTrainingQuestion"),
            Loc.T("TrainingNotSaved"), Loc.T("EndBtn"), Loc.T("ContinueBtn"));
        if (opravdu)
        {
            _timer?.Stop();
            await Shell.Current.GoToAsync("..");
        }
    }
}
