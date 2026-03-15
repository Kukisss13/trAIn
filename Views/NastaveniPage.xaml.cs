using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

public partial class NastaveniPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly ThemeService _theme;
    private readonly ClaudeApiService _claude;
    private Uzivatel? _uzivatel;
    private Uzivatel? _profil;
    private string _pohlavi = "";
    private string _cil = "";
    private string _uroven = "";

    public NastaveniPage(DatabaseService db, ThemeService theme, ClaudeApiService claude)
    {
        InitializeComponent();
        _db = db;
        _theme = theme;
        _claude = claude;

        ThemeSwitch.IsToggled = _theme.JeTmavy;
        AktualizujThemeLabely();
        AktualizujJazykButtony();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
        _uzivatel = await _db.GetPrihlasenyUzivatelAsync();
        AktualizujUI();
        AktualizujApiKeyStatus();
        await NactiProfil();
    }

    private void AktualizujApiKeyStatus()
    {
        if (_claude.MaApiKlic)
        {
            var klic = _claude.ApiKey;
            var maskovan = klic.Length > 8
                ? klic[..4] + "..." + klic[^4..]
                : "****";
            ApiKeyStatusLabel.Text = string.Format(Loc.T("KeySet"), maskovan);
            ApiKeyStatusLabel.TextColor = Colors.LimeGreen;
        }
        else
        {
            ApiKeyStatusLabel.Text = Loc.T("KeyNotSet");
            ApiKeyStatusLabel.TextColor = Color.FromArgb("#919191");
        }
    }

    private async void OnUlozitApiKeyClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);

        var klic = ApiKeyEntry.Text?.Trim();
        if (string.IsNullOrEmpty(klic))
        {
            await DisplayAlert(Loc.T("Error"), Loc.T("EnterApiKey"), Loc.T("OK"));
            return;
        }

        _claude.ApiKey = klic;
        ApiKeyEntry.Text = "";
        AktualizujApiKeyStatus();
        await DisplayAlert(Loc.T("Saved"), Loc.T("ApiKeySaved"), Loc.T("OK"));
    }

    private void AktualizujUI()
    {
        if (_uzivatel != null)
        {
            NeprihlasenyPanel.IsVisible = false;
            PrihlasenyPanel.IsVisible = true;

            JmenoLabel.Text = _uzivatel.Jmeno;
            KontaktLabel.Text = !string.IsNullOrEmpty(_uzivatel.Email)
                ? _uzivatel.Email
                : _uzivatel.Telefon;

            var metoda = _uzivatel.MetodaPrihlaseni switch
            {
                "email" => Loc.T("Email"),
                "telefon" => Loc.T("Phone"),
                "apple" => Loc.T("AppleId"),
                _ => ""
            };
            MetodaLabel.Text = string.Format(Loc.T("SignedVia"), metoda);

            AvatarLabel.Text = !string.IsNullOrEmpty(_uzivatel.Jmeno)
                ? _uzivatel.Jmeno[0].ToString().ToUpper()
                : "?";
        }
        else
        {
            NeprihlasenyPanel.IsVisible = true;
            PrihlasenyPanel.IsVisible = false;
        }
    }

    private async void OnEmailClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("registrace?metoda=email");
    }

    private async void OnTelefonClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("registrace?metoda=telefon");
    }

    private async void OnAppleClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("registrace?metoda=apple");
    }

    private async void OnOdhlasitClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        bool odhlasit = await DisplayAlert(Loc.T("SignOut"),
            Loc.T("SignOutConfirm"), Loc.T("SignOut"), Loc.T("Cancel"));
        if (!odhlasit) return;

        await _db.OdhlasUzivateleAsync();
        _uzivatel = null;
        AktualizujUI();
    }

    private void OnThemeToggled(object? sender, ToggledEventArgs e)
    {
        _theme.JeTmavy = e.Value;
        AktualizujThemeLabely();
    }

    private void AktualizujThemeLabely()
    {
        bool tmavy = _theme.JeTmavy;
        ThemeLabel.Text = tmavy ? Loc.T("DarkMode") : Loc.T("LightMode");
        ThemeSubLabel.Text = Loc.T("Active");
        ThemeIcon.Text = tmavy ? "\U0001F319" : "\u2600";
    }

    // === JAZYK ===

    private void OnCzechClicked(object? sender, EventArgs e)
    {
        Loc.Instance.SetLanguage("cs");
        AktualizujJazykButtony();
    }

    private void OnEnglishClicked(object? sender, EventArgs e)
    {
        Loc.Instance.SetLanguage("en");
        AktualizujJazykButtony();
    }

    private void AktualizujJazykButtony()
    {
        var active = Application.Current?.Resources["Primary"] as Color ?? Colors.Purple;
        var inactive = Color.FromArgb("#2A2A2A");
        bool jeCz = Loc.Instance.Language == "cs";

        BtnCzech.BackgroundColor = jeCz ? active : inactive;
        BtnCzech.TextColor = jeCz ? Colors.White : Color.FromArgb("#888888");
        BtnEnglish.BackgroundColor = !jeCz ? active : inactive;
        BtnEnglish.TextColor = !jeCz ? Colors.White : Color.FromArgb("#888888");

        LanguageLabel.Text = jeCz ? Loc.T("Czech") : Loc.T("English");

        AktualizujThemeLabely();
        AktualizujApiKeyStatus();
    }

    // === PROFIL ===

    private async Task NactiProfil()
    {
        _profil = await _db.GetNeboVytvorProfilAsync();

        ProfilJmenoEntry.Text = _profil.Jmeno;
        ProfilVekEntry.Text = _profil.Vek > 0 ? _profil.Vek.ToString() : "";
        ProfilVahaEntry.Text = _profil.Vaha > 0 ? _profil.Vaha.ToString() : "";
        ProfilVyskaEntry.Text = _profil.Vyska > 0 ? _profil.Vyska.ToString() : "";

        _pohlavi = _profil.Pohlavi;
        _cil = _profil.Cil;
        _uroven = _profil.UrovenAktivity;

        AktualizujPohlaviButtony();
        AktualizujCilButtony();
        AktualizujUrovenButtony();
        AktualizujBmi();
    }

    private void OnPohlaviClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        _pohlavi = btn == BtnMuz ? "muz" : "zena";
        AktualizujPohlaviButtony();
    }

    private void OnCilClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        _cil = btn.ClassId;
        AktualizujCilButtony();
    }

    private void OnUrovenClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        _uroven = btn.ClassId;
        AktualizujUrovenButtony();
    }

    private void AktualizujPohlaviButtony()
    {
        var active = Application.Current?.Resources["Primary"] as Color ?? Colors.Purple;
        var inactive = Color.FromArgb("#2A2A2A");

        BtnMuz.BackgroundColor = _pohlavi == "muz" ? active : inactive;
        BtnMuz.TextColor = _pohlavi == "muz" ? Colors.White : Color.FromArgb("#888888");
        BtnZena.BackgroundColor = _pohlavi == "zena" ? active : inactive;
        BtnZena.TextColor = _pohlavi == "zena" ? Colors.White : Color.FromArgb("#888888");
    }

    private void AktualizujCilButtony()
    {
        var active = Application.Current?.Resources["Primary"] as Color ?? Colors.Purple;
        var inactive = Color.FromArgb("#2A2A2A");

        foreach (var (btn, id) in new[] {
            (BtnCilNabrat, "nabrat"), (BtnCilZhubnout, "zhubnout"),
            (BtnCilUdrzovat, "udrzovat"), (BtnCilKondice, "kondice") })
        {
            btn.BackgroundColor = _cil == id ? active : inactive;
            btn.TextColor = _cil == id ? Colors.White : Color.FromArgb("#888888");
        }
    }

    private void AktualizujUrovenButtony()
    {
        var active = Application.Current?.Resources["Primary"] as Color ?? Colors.Purple;
        var inactive = Color.FromArgb("#2A2A2A");

        foreach (var (btn, id) in new[] {
            (BtnUrovenZacatecnik, "zacatecnik"), (BtnUrovenPokrocily, "pokrocily"),
            (BtnUrovenExpert, "expert") })
        {
            btn.BackgroundColor = _uroven == id ? active : inactive;
            btn.TextColor = _uroven == id ? Colors.White : Color.FromArgb("#888888");
        }
    }

    private void AktualizujBmi()
    {
        if (double.TryParse(ProfilVahaEntry.Text, out double vaha) &&
            double.TryParse(ProfilVyskaEntry.Text, out double vyska) &&
            vaha > 0 && vyska > 0)
        {
            double vyskaM = vyska / 100.0;
            double bmi = vaha / (vyskaM * vyskaM);
            BmiLabel.Text = bmi.ToString("F1");
            BmiKategorieLabel.Text = bmi switch
            {
                < 18.5 => Loc.T("Underweight"),
                < 25 => Loc.T("Normal"),
                < 30 => Loc.T("Overweight"),
                _ => Loc.T("Obese")
            };
            BmiFrame.IsVisible = true;
        }
        else
        {
            BmiFrame.IsVisible = false;
        }
    }

    private async void OnUlozitProfilClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        if (_profil == null) return;

        _profil.Jmeno = ProfilJmenoEntry.Text?.Trim() ?? "";
        _profil.Vek = int.TryParse(ProfilVekEntry.Text, out var vek) ? vek : 0;
        _profil.Vaha = double.TryParse(ProfilVahaEntry.Text, out var vaha) ? vaha : 0;
        _profil.Vyska = double.TryParse(ProfilVyskaEntry.Text, out var vyska) ? vyska : 0;
        _profil.Pohlavi = _pohlavi;
        _profil.Cil = _cil;
        _profil.UrovenAktivity = _uroven;

        await _db.UlozUzivateleAsync(_profil);
        AktualizujBmi();

        await DisplayAlert(Loc.T("Saved"), Loc.T("ProfileSaved"), Loc.T("OK"));
    }

    private async void OnZasobaCvikuClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("cviceni");
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}
