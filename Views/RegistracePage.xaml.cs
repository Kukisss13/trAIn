using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

[QueryProperty(nameof(Metoda), "metoda")]
public partial class RegistracePage : ContentPage
{
    private readonly DatabaseService _db;
    private string _metoda = "email";

    public string Metoda
    {
        set => _metoda = value ?? "email";
    }

    public RegistracePage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();
        NastavFormular();
    }

    private void NastavFormular()
    {
        EmailPanel.IsVisible = _metoda == "email";
        TelefonPanel.IsVisible = _metoda == "telefon";
        ApplePanel.IsVisible = _metoda == "apple";

        switch (_metoda)
        {
            case "email":
                HlavickaLabel.Text = Loc.T("EmailSignIn");
                MetodaIkona.Text = "\u2709";
                PopisLabel.Text = Loc.T("EnterNameEmail");
                PotvrditButton.Text = Loc.T("Continue");
                break;
            case "telefon":
                HlavickaLabel.Text = Loc.T("PhoneSignIn");
                MetodaIkona.Text = "\U0001F4F1";
                PopisLabel.Text = Loc.T("EnterNamePhone");
                PotvrditButton.Text = Loc.T("Continue");
                break;
            case "apple":
                HlavickaLabel.Text = Loc.T("AppleSignIn");
                MetodaIkona.Text = "\uF8FF";
                PopisLabel.Text = Loc.T("EnterNameApple");
                PotvrditButton.Text = Loc.T("SignInAppleBtn");
                break;
        }
    }

    private async void OnPotvrditClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        var jmeno = JmenoEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(jmeno))
        {
            await DisplayAlert(Loc.T("Error"), Loc.T("EnterYourName"), Loc.T("OK"));
            return;
        }

        if (_metoda == "email")
        {
            var email = EmailEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                await DisplayAlert(Loc.T("Error"), Loc.T("EnterValidEmail"), Loc.T("OK"));
                return;
            }

            var uzivatel = new Uzivatel
            {
                Jmeno = jmeno,
                Email = email,
                MetodaPrihlaseni = "email",
            };
            await _db.UlozUzivateleAsync(uzivatel);
        }
        else if (_metoda == "telefon")
        {
            var telefon = TelefonEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(telefon) || telefon.Length < 9)
            {
                await DisplayAlert(Loc.T("Error"), Loc.T("EnterValidPhone"), Loc.T("OK"));
                return;
            }

            var uzivatel = new Uzivatel
            {
                Jmeno = jmeno,
                Telefon = telefon,
                MetodaPrihlaseni = "telefon",
            };
            await _db.UlozUzivateleAsync(uzivatel);
        }
        else if (_metoda == "apple")
        {
            var uzivatel = new Uzivatel
            {
                Jmeno = jmeno,
                MetodaPrihlaseni = "apple",
            };
            await _db.UlozUzivateleAsync(uzivatel);
        }

        await DisplayAlert(Loc.T("Done"), string.Format(Loc.T("WelcomeUser"), jmeno), Loc.T("OK"));
        await Shell.Current.GoToAsync("../..");
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}
