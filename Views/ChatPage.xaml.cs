using System.Collections.ObjectModel;
using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

public partial class ChatPage : ContentPage
{
    private readonly ClaudeApiService _claude;
    private readonly DatabaseService _db;
    private readonly ObservableCollection<ChatZprava> _zpravy = new();
    private string _systemPrompt = string.Empty;
    private bool _odesilaSe;

    public ChatPage(ClaudeApiService claude, DatabaseService db)
    {
        InitializeComponent();
        _claude = claude;
        _db = db;
        ZpravyCollection.ItemsSource = _zpravy;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();

        _systemPrompt = await _claude.SestrojSystemPromptAsync();

        var historie = await _db.GetChatHistoriiAsync();
        _zpravy.Clear();
        foreach (var z in historie)
            _zpravy.Add(z);
    }

    private async void OnOdeslatClicked(object? sender, EventArgs e)
    {
        var text = ZpravaEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text) || _odesilaSe)
            return;

        await OdeslatZpravu(text);
    }

    private async void OnNavrhClicked(object? sender, EventArgs e)
    {
        if (sender is Frame f)
            await AnimaceHelper.AnimovatKlik(f);

        if (sender is BindableObject bo)
        {
            var tapGesture = (bo as Frame)?.GestureRecognizers
                .OfType<TapGestureRecognizer>()
                .FirstOrDefault();
            var text = tapGesture?.CommandParameter as string;
            if (!string.IsNullOrEmpty(text) && !_odesilaSe)
                await OdeslatZpravu(text);
        }
    }

    private async Task OdeslatZpravu(string text)
    {
        _odesilaSe = true;
        ZpravaEntry.Text = string.Empty;
        LoadingPanel.IsVisible = true;

        var userZprava = new ChatZprava
        {
            Role = "user",
            Obsah = text,
            Cas = DateTime.Now
        };
        _zpravy.Add(userZprava);
        await _db.UlozChatZpravuAsync(userZprava);

        try
        {
            var historieProApi = _zpravy.ToList();
            var odpoved = await _claude.PoslatZpravuAsync(historieProApi, _systemPrompt);

            var aiZprava = new ChatZprava
            {
                Role = "assistant",
                Obsah = odpoved,
                Cas = DateTime.Now
            };
            _zpravy.Add(aiZprava);
            await _db.UlozChatZpravuAsync(aiZprava);
        }
        catch (Exception ex)
        {
            var errorZprava = new ChatZprava
            {
                Role = "assistant",
                Obsah = string.Format(Loc.T("ErrGeneric"), ex.Message),
                Cas = DateTime.Now
            };
            _zpravy.Add(errorZprava);
        }
        finally
        {
            LoadingPanel.IsVisible = false;
            _odesilaSe = false;
        }
    }

    private async void OnSmazatHistoriiClicked(object? sender, EventArgs e)
    {
        if (sender is View v)
            await AnimaceHelper.AnimovatKlik(v);

        if (_zpravy.Count == 0)
            return;

        var potvrdit = await DisplayAlert(Loc.T("DeleteHistory"),
            Loc.T("DeleteHistoryConfirm"), Loc.T("Delete"), Loc.T("Cancel"));

        if (potvrdit)
        {
            await _db.SmazChatHistoriiAsync();
            _zpravy.Clear();
        }
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v)
            await AnimaceHelper.AnimovatKlik(v);
        await Shell.Current.GoToAsync("..");
    }
}
