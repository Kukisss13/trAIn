using System.Collections.ObjectModel;
using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

[QueryProperty(nameof(PlanIdStr), "planId")]
public partial class TreninkTabulkaPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly ClaudeApiService _claude;
    private readonly List<RadekTabulky> _radky = new();
    private List<Cviceni> _vsechnyCviky = new();

    // Chat
    private readonly ObservableCollection<ChatZprava> _zpravy = new();
    private string _systemPrompt = string.Empty;
    private bool _odesilaSe;
    private int _planId;
    private TreninkovyPlan? _plan;

    // Barvy tabulky
    private static readonly Color HraniceBarva = Color.FromArgb("#4A4A6A");
    private static readonly Color HlavickaBarva = Color.FromArgb("#252545");
    private static readonly Color BunkaBarva = Color.FromArgb("#1A1A2E");
    private static readonly Color BunkaSudaBarva = Color.FromArgb("#1F1F38");
    private static readonly Color TextBarva = Colors.White;
    private static readonly Color TextSedaBarva = Color.FromArgb("#CCCCCC");
    private static readonly Color AkcentBarva = Color.FromArgb("#FF6B35");
    private static readonly Color NabidkaBarva = Color.FromArgb("#2A2A4A");

    // Autocomplete stav
    private VerticalStackLayout? _nabidkaPanel;
    private int _vychoziPocetSerii = 3;
    private bool _zobrazitPauzu = true;

    // Auto-save
    private CancellationTokenSource? _autoSaveCts;

    // Ochrana proti rekurzi při formátování série
    private bool _formatujeSerii = false;

    public string PlanIdStr
    {
        set => _planId = int.TryParse(value, out var id) ? id : 0;
    }

    public TreninkTabulkaPage(DatabaseService db, ClaudeApiService claude)
    {
        InitializeComponent();
        _db = db;
        _claude = claude;
        ZpravyCollection.ItemsSource = _zpravy;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loc.Instance.RefreshBindings();

        // Načti plán z DB
        _plan = await _db.GetPlanByIdAsync(_planId);
        if (_plan == null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        NazevLabel.Text = _plan.Nazev;
        DatumLabel.Text = _plan.Datum.ToString("d. MMMM yyyy");

        // Načti všechny cviky pro autocomplete
        _vsechnyCviky = await _db.GetCviceniAsync();

        // Načti existující položky plánu (pokud už nějaké jsou)
        if (_radky.Count == 0)
        {
            var existujici = await _db.GetPolozkyPlanuAsync(_planId);
            if (existujici.Count > 0)
            {
                foreach (var p in existujici)
                {
                    var serieData = new List<string>();
                    if (!string.IsNullOrEmpty(p.OpakovaniDetail))
                        serieData.AddRange(p.OpakovaniDetail.Split(';'));
                    while (serieData.Count < p.Serie)
                        serieData.Add("");

                    _radky.Add(new RadekTabulky
                    {
                        CviceniId = p.CviceniId,
                        NazevCviku = p.NazevCviku,
                        Serie = p.Serie,
                        SerieData = serieData,
                        Odpocinek = p.Pauza > 0 ? p.Pauza.ToString() : "90",
                        Poznamka = p.Poznamka,
                    });
                }
            }
            else
            {
                _radky.Add(VytvorPrazdnyRadek());
            }
        }

        AktualizujTabulku();

        // Načti chat
        _systemPrompt = await _claude.SestrojSystemPromptAsync();
        var historie = await _db.GetChatHistoriiAsync();
        _zpravy.Clear();
        foreach (var z in historie)
            _zpravy.Add(z);
    }

    private RadekTabulky VytvorPrazdnyRadek()
    {
        var data = new List<string>();
        for (int i = 0; i < _vychoziPocetSerii; i++) data.Add("");
        return new RadekTabulky
        {
            CviceniId = 0,
            NazevCviku = "",
            Serie = _vychoziPocetSerii,
            SerieData = data,
            Odpocinek = "90",
            Poznamka = "",
        };
    }

    /// <summary>
    /// Naplánuje auto-save za 2 sekundy od poslední změny
    /// </summary>
    private void NaplanovatAutoSave()
    {
        _autoSaveCts?.Cancel();
        _autoSaveCts = new CancellationTokenSource();
        var token = _autoSaveCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000, token);
                if (!token.IsCancellationRequested)
                    await UlozDoDb();
            }
            catch (TaskCanceledException) { }
        });
    }

    /// <summary>
    /// Uloží aktuální stav řádků do databáze
    /// </summary>
    private async Task UlozDoDb()
    {
        if (_planId == 0) return;

        // Smaž staré položky a ulož nové
        await _db.SmazPolozkyPlanuAsync(_planId);

        var kUlozeni = _radky.Where(r => !string.IsNullOrWhiteSpace(r.NazevCviku)).ToList();
        for (int i = 0; i < kUlozeni.Count; i++)
        {
            var r = kUlozeni[i];
            var polozka = new PolozkaPlanu
            {
                TreninkovyPlanId = _planId,
                CviceniId = r.CviceniId,
                NazevCviku = r.NazevCviku,
                Serie = r.Serie,
                OpakovaniDetail = string.Join(";", r.SerieData),
                Vaha = 0,
                Pauza = int.TryParse(r.Odpocinek, out var p) ? p : 0,
                Poznamka = r.Poznamka,
                Poradi = i + 1,
            };
            await _db.UlozPolozkuAsync(polozka);
        }
    }

    private void AktualizujTabulku()
    {
        TabulkaStack.Children.Clear();
        SkryjNabidky();

        int maxSerii = _radky.Count > 0 ? _radky.Max(r => r.Serie) : 3;
        if (maxSerii < 1) maxSerii = 3;

        // Hlavička
        TabulkaStack.Children.Add(VytvorHlavicku(maxSerii));

        // Řádky
        for (int i = 0; i < _radky.Count; i++)
        {
            TabulkaStack.Children.Add(VytvorRadek(_radky[i], i, maxSerii, i % 2 == 1));
        }

        // + tlačítko pro přidání řádku (max 30)
        if (_radky.Count < 30)
        {
            var plusRadek = new Grid
            {
                HeightRequest = 40,
                BackgroundColor = BunkaBarva,
            };
            var plusBorder = new Border
            {
                Stroke = HraniceBarva,
                StrokeThickness = 1,
                BackgroundColor = BunkaBarva,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 0 },
                Content = new Label
                {
                    Text = "+",
                    FontSize = 22,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = AkcentBarva,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                },
            };
            var plusTap = new TapGestureRecognizer();
            plusTap.Tapped += (s, e) =>
            {
                _radky.Add(VytvorPrazdnyRadek());
                AktualizujTabulku();
                NaplanovatAutoSave();
            };
            plusBorder.GestureRecognizers.Add(plusTap);
            TabulkaStack.Children.Add(plusBorder);
        }
    }

    private View VytvorHlavicku(int maxSerii)
    {
        var grid = VytvorTabulkovyGrid(maxSerii);

        int col = 0;
        PridatHlavickuBunku(grid, col++, Loc.T("ExerciseNameCol"), TextAlignment.Start);
        for (int s = 0; s < maxSerii; s++)
            PridatHlavickuBunku(grid, col++, string.Format(Loc.T("SeriesCol"), s + 1));
        PridatHlavickuBunku(grid, col++, ""); // + tlačítko
        if (_zobrazitPauzu)
        {
            PridatHlavickuBunku(grid, col++, Loc.T("RestCol"));
        }
        PridatHlavickuBunku(grid, col++, Loc.T("NotesCol"));
        PridatHlavickuBunku(grid, col++, ""); // smazat

        return grid;
    }

    private View VytvorRadek(RadekTabulky radek, int index, int maxSerii, bool suda)
    {
        var bgColor = suda ? BunkaSudaBarva : BunkaBarva;

        // Wrapper pro řádek + nabídky pod ním
        var wrapper = new VerticalStackLayout { Spacing = 0 };

        var grid = VytvorTabulkovyGrid(maxSerii);
        int col = 0;

        // === Cvik — Entry s autocomplete ===
        var cvikEntry = new Entry
        {
            Text = radek.NazevCviku,
            FontSize = 13,
            TextColor = AkcentBarva,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Placeholder = Loc.T("ExerciseNamePlaceholderCol"),
            PlaceholderColor = Color.FromArgb("#555555"),
            Keyboard = Keyboard.Default,
            HeightRequest = 38,
        };

        // Panel pro nabídky tohoto řádku
        var nabidkaPanel = new VerticalStackLayout
        {
            Spacing = 0,
            IsVisible = false,
            BackgroundColor = NabidkaBarva,
        };

        cvikEntry.TextChanged += (s, e) =>
        {
            radek.NazevCviku = e.NewTextValue ?? "";
            radek.CviceniId = 0;
            AktualizujNabidky(nabidkaPanel, radek, e.NewTextValue ?? "");
            NaplanovatAutoSave();
        };

        cvikEntry.Focused += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(radek.NazevCviku))
                AktualizujNabidky(nabidkaPanel, radek, radek.NazevCviku);
        };

        cvikEntry.Unfocused += (s, e) =>
        {
            // Malý delay aby klik na nabídku stihl proběhnout
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
            {
                nabidkaPanel.IsVisible = false;
            });
        };

        grid.Children.Add(VytvorBunku(cvikEntry, col++, bgColor));

        // === Série 1, 2, 3... ===
        for (int s = 0; s < maxSerii; s++)
        {
            if (s < radek.Serie)
            {
                int si = s;
                // Zobraz existující data s formátem (pokud mají "opak/vaha", přidej "kg")
                var zobrazText = FormatujSeriiText(radek.SerieData[s]);
                var entry = VytvorCellEntry(zobrazText, "opak/kg", Keyboard.Default);
                entry.TextChanged += (sender, e) =>
                {
                    if (_formatujeSerii) return;
                    FormatujSeriiEntry((Entry)sender!, si, radek);
                    NaplanovatAutoSave();
                };
                // Enter → automaticky vloží "/" pokud ještě není
                entry.Completed += (sender, e) =>
                {
                    var ent = (Entry)sender!;
                    var txt = ent.Text ?? "";
                    if (txt.Length > 0 && !txt.Contains('/'))
                    {
                        _formatujeSerii = true;
                        ent.Text = txt + "/";
                        ent.CursorPosition = ent.Text.Length;
                        if (si < radek.SerieData.Count)
                            radek.SerieData[si] = txt + "/";
                        _formatujeSerii = false;
                        // Vrať focus zpět do stejného pole (Enter ho normálně odebere)
                        ent.Focus();
                    }
                };
                grid.Children.Add(VytvorBunku(entry, col++, bgColor));
            }
            else
            {
                grid.Children.Add(VytvorBunku(new BoxView { Color = Colors.Transparent }, col++, bgColor));
            }
        }

        // === + tlačítko (max 10) ===
        var plusLabel = new Label
        {
            Text = radek.Serie < 10 ? "+" : "",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = AkcentBarva,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        if (radek.Serie < 10)
        {
            var plusTap = new TapGestureRecognizer();
            plusTap.Tapped += (s, e) =>
            {
                radek.Serie++;
                radek.SerieData.Add("");
                AktualizujTabulku();
                NaplanovatAutoSave();
            };
            plusLabel.GestureRecognizers.Add(plusTap);
        }
        grid.Children.Add(VytvorBunku(plusLabel, col++, bgColor));

        // === Pauzy ===
        if (_zobrazitPauzu)
        {
            var pauzaZobraz = !string.IsNullOrEmpty(radek.Odpocinek) && radek.Odpocinek.Any(char.IsDigit)
                ? radek.Odpocinek.Replace("s", "") + "s"
                : radek.Odpocinek;
            var pauzaEntry = VytvorCellEntry(pauzaZobraz, "s");
            pauzaEntry.TextChanged += (s, e) =>
            {
                if (_formatujeSerii) return;
                _formatujeSerii = true;
                try
                {
                    var txt = e.NewTextValue ?? "";
                    // Odstraň "s" a nečíselné znaky
                    var cislo = new string(txt.Where(char.IsDigit).ToArray());
                    var zobraz = cislo.Length > 0 ? cislo + "s" : "";
                    if (((Entry)s!).Text != zobraz)
                    {
                        ((Entry)s!).Text = zobraz;
                        if (zobraz.EndsWith("s"))
                            ((Entry)s!).CursorPosition = zobraz.Length - 1;
                    }
                    radek.Odpocinek = cislo;
                }
                finally { _formatujeSerii = false; }
                NaplanovatAutoSave();
            };
            grid.Children.Add(VytvorBunku(pauzaEntry, col++, bgColor));
        }

        // === Poznámky ===
        var pozEntry = VytvorCellEntry(radek.Poznamka, "...", Keyboard.Default);
        pozEntry.TextChanged += (s, e) =>
        {
            radek.Poznamka = e.NewTextValue ?? "";
            NaplanovatAutoSave();
        };
        grid.Children.Add(VytvorBunku(pozEntry, col++, bgColor));

        // === Smazat ===
        var smazatLabel = new Label
        {
            Text = "✕",
            FontSize = 18,
            TextColor = Color.FromArgb("#E94560"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var smazatTap = new TapGestureRecognizer();
        smazatTap.Tapped += (s, e) =>
        {
            if (_radky.Count > 1 || !string.IsNullOrWhiteSpace(radek.NazevCviku))
            {
                _radky.RemoveAt(index);
                AktualizujTabulku();
                NaplanovatAutoSave();
            }
        };
        smazatLabel.GestureRecognizers.Add(smazatTap);
        grid.Children.Add(VytvorBunku(smazatLabel, col++, bgColor));

        wrapper.Children.Add(grid);
        wrapper.Children.Add(nabidkaPanel);

        return wrapper;
    }

    /// <summary>
    /// Aktualizuje nabídky cviků pod daným řádkem
    /// </summary>
    private void AktualizujNabidky(VerticalStackLayout panel, RadekTabulky radek, string hledanyText)
    {
        panel.Children.Clear();

        if (string.IsNullOrWhiteSpace(hledanyText) || hledanyText.Length < 1)
        {
            panel.IsVisible = false;
            return;
        }

        var text = hledanyText.ToLowerInvariant();
        var nalezene = _vsechnyCviky
            .Where(c => c.Nazev.ToLowerInvariant().Contains(text))
            .Take(6)
            .ToList();

        if (nalezene.Count == 0 || (nalezene.Count == 1 && nalezene[0].Nazev.Equals(hledanyText, StringComparison.OrdinalIgnoreCase)))
        {
            panel.IsVisible = false;
            return;
        }

        foreach (var cvik in nalezene)
        {
            var radekNabidky = new Grid
            {
                Padding = new Thickness(10, 8),
                BackgroundColor = NabidkaBarva,
            };

            var label = new Label
            {
                FontSize = 13,
                TextColor = TextBarva,
                VerticalOptions = LayoutOptions.Center,
            };

            // Zvýrazni hledaný text
            var fs = new FormattedString();
            var nazev = cvik.Nazev;
            var idx = nazev.ToLowerInvariant().IndexOf(text);
            if (idx >= 0)
            {
                if (idx > 0)
                    fs.Spans.Add(new Span { Text = nazev[..idx], TextColor = TextSedaBarva });
                fs.Spans.Add(new Span { Text = nazev[idx..(idx + text.Length)], TextColor = AkcentBarva, FontAttributes = FontAttributes.Bold });
                if (idx + text.Length < nazev.Length)
                    fs.Spans.Add(new Span { Text = nazev[(idx + text.Length)..], TextColor = TextSedaBarva });
            }
            else
            {
                fs.Spans.Add(new Span { Text = nazev, TextColor = TextSedaBarva });
            }
            label.FormattedText = fs;

            radekNabidky.Children.Add(label);

            // Spodní linka
            var linka = new BoxView { Color = HraniceBarva, HeightRequest = 1, VerticalOptions = LayoutOptions.End };
            radekNabidky.Children.Add(linka);

            var tap = new TapGestureRecognizer();
            var vybrany = cvik;
            tap.Tapped += (s, e) =>
            {
                radek.NazevCviku = vybrany.Nazev;
                radek.CviceniId = vybrany.Id;
                panel.IsVisible = false;
                AktualizujTabulku();
                NaplanovatAutoSave();
            };
            radekNabidky.GestureRecognizers.Add(tap);

            panel.Children.Add(radekNabidky);
        }

        panel.IsVisible = true;
    }

    private void SkryjNabidky()
    {
        _nabidkaPanel?.IsVisible.Equals(false);
        _nabidkaPanel = null;
    }

    private Grid VytvorTabulkovyGrid(int maxSerii)
    {
        var grid = new Grid
        {
            ColumnSpacing = 0,
            RowSpacing = 0,
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(170))); // Cvik
        for (int s = 0; s < maxSerii; s++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(90))); // Série 1,2,3...
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(42)));  // + tlačítko
        if (_zobrazitPauzu)
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(65)));  // Pauza
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(130))); // Poznámky
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(40)));  // Smazat

        return grid;
    }

    private Border VytvorBunku(View obsah, int col, Color bgColor)
    {
        var border = new Border
        {
            Stroke = HraniceBarva,
            StrokeThickness = 1,
            BackgroundColor = bgColor,
            Padding = new Thickness(2),
            Content = obsah,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 0 },
        };
        Grid.SetColumn(border, col);
        return border;
    }

    private void PridatHlavickuBunku(Grid grid, int col, string text, TextAlignment align = TextAlignment.Center)
    {
        var label = new Label
        {
            Text = text,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = TextSedaBarva,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = align,
            Padding = new Thickness(4, 10),
        };
        grid.Children.Add(VytvorBunku(label, col, HlavickaBarva));
    }

    private Entry VytvorCellEntry(string text, string placeholder = "", Keyboard? keyboard = null)
    {
        return new Entry
        {
            Text = text,
            FontSize = 14,
            TextColor = TextBarva,
            BackgroundColor = Colors.Transparent,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Keyboard = keyboard ?? Keyboard.Numeric,
            Placeholder = placeholder,
            PlaceholderColor = Color.FromArgb("#555555"),
            HeightRequest = 38,
        };
    }

    /// <summary>
    /// Formátuje text série: auto-vloží "/" a "kg"
    /// Uživatel píše čísla, "/" se vloží automaticky po prvním čísle,
    /// "kg" se dopíše za váhu.
    /// </summary>
    private void FormatujSeriiEntry(Entry entry, int serieIndex, RadekTabulky radek)
    {
        _formatujeSerii = true;
        try
        {
            var text = entry.Text ?? "";

            // Odstraň "kg" pro čistou práci
            var raw = text.Replace("kg", "");

            // Povol pouze číslice, jedno "/" a tečku/čárku pro desetinná čísla
            var cleaned = "";
            bool hasSlash = false;
            foreach (var c in raw)
            {
                if (char.IsDigit(c))
                    cleaned += c;
                else if (c == '/' && !hasSlash)
                {
                    cleaned += c;
                    hasSlash = true;
                }
                else if ((c == '.' || c == ',') && cleaned.Length > 0)
                    cleaned += c;
            }

            // Sestav formátovaný text
            string formatted;
            var parts = cleaned.Split('/');
            if (parts.Length == 2 && parts[1].Length > 0)
            {
                // Máme opak i váhu → přidej "kg"
                formatted = cleaned + "kg";
            }
            else if (parts.Length == 2)
            {
                // Máme "/" ale ještě žádnou váhu
                formatted = cleaned;
            }
            else
            {
                // Zatím jen číslo opakovánÍ
                formatted = cleaned;
            }

            if (entry.Text != formatted)
            {
                entry.Text = formatted;
                // Kurzor před "kg"
                if (formatted.EndsWith("kg"))
                    entry.CursorPosition = formatted.Length - 2;
                else
                    entry.CursorPosition = formatted.Length;
            }

            // Ulož čistou hodnotu bez "kg" do dat
            if (serieIndex < radek.SerieData.Count)
                radek.SerieData[serieIndex] = cleaned;
        }
        finally
        {
            _formatujeSerii = false;
        }
    }

    /// <summary>
    /// Zformátuje existující text série pro zobrazení (přidá "kg" pokud má formát "opak/vaha")
    /// </summary>
    private static string FormatujSeriiText(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        var parts = raw.Replace("kg", "").Split('/');
        if (parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0)
            return raw.Replace("kg", "") + "kg";
        return raw;
    }

    private async void OnUlozitClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        // Zruš případný pending auto-save a ulož hned
        _autoSaveCts?.Cancel();
        await UlozDoDb();

        // Označ jako hotový (ne koncept)
        if (_plan != null)
        {
            _plan.JeKoncept = false;
            await _db.UlozPlanAsync(_plan);
        }

        var kUlozeni = _radky.Where(r => !string.IsNullOrWhiteSpace(r.NazevCviku)).ToList();
        await DisplayAlert(Loc.T("Saved"), string.Format(Loc.T("TrainingSaved"), _plan?.Nazev, kUlozeni.Count), Loc.T("OK"));
        await Shell.Current.GoToAsync("../..");
    }

    private async void OnZpetClicked(object? sender, EventArgs e)
    {
        if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
        _autoSaveCts?.Cancel();
        await UlozDoDb();
        await Shell.Current.GoToAsync("..");
    }

    // === Chat metody ===

    private async void OnOdeslatChatClicked(object? sender, EventArgs e)
    {
        var text = ChatVstup.Text?.Trim();
        if (string.IsNullOrEmpty(text) || _odesilaSe)
            return;
        await OdeslatZpravu(text);
    }

    private async void OnNavrhClicked(object? sender, EventArgs e)
    {
        if (sender is Frame f)
            await AnimaceHelper.AnimovatKlik(f);

        var tapGesture = (sender as Frame)?.GestureRecognizers
            .OfType<TapGestureRecognizer>()
            .FirstOrDefault();
        var text = tapGesture?.CommandParameter as string;
        if (!string.IsNullOrEmpty(text) && !_odesilaSe)
            await OdeslatZpravu(text);
    }

    private async Task OdeslatZpravu(string text)
    {
        _odesilaSe = true;
        ChatVstup.Text = string.Empty;
        ChatLoadingPanel.IsVisible = true;

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
                Obsah = $"{Loc.T("Error")}: {ex.Message}",
                Cas = DateTime.Now
            };
            _zpravy.Add(errorZprava);
        }
        finally
        {
            ChatLoadingPanel.IsVisible = false;
            _odesilaSe = false;
        }
    }

    private async void OnSmazatChatClicked(object? sender, EventArgs e)
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
}

/// <summary>
/// Interní model řádku tabulky
/// </summary>
public class RadekTabulky
{
    public int CviceniId { get; set; }
    public string NazevCviku { get; set; } = string.Empty;
    public int Serie { get; set; } = 3;
    /// <summary>
    /// Každý prvek = "opak/kg" pro danou sérii, např. "12/80"
    /// </summary>
    public List<string> SerieData { get; set; } = new() { "", "", "" };
    public string Odpocinek { get; set; } = "90";
    public string Poznamka { get; set; } = string.Empty;
}
