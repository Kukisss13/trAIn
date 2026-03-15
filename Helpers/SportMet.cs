namespace TreninkovyPlanovac.Helpers;

public static class SportMet
{
    private static readonly Dictionary<string, (double Met, string Nazev, string Ikona)> _sporty = new()
    {
        ["beh"]        = (9.8,  "Běh",        "\U0001F3C3"),
        ["kolo"]       = (7.5,  "Kolo",       "\U0001F6B4"),
        ["plavani"]    = (8.0,  "Plavání",    "\U0001F3CA"),
        ["posilovna"]  = (5.0,  "Posilovna",  "\U0001F4AA"),
        ["hiit"]       = (10.0, "HIIT",       "\u26A1"),
        ["crossfit"]   = (9.0,  "CrossFit",   "\U0001F525"),
        ["fotbal"]     = (7.0,  "Fotbal",     "\u26BD"),
        ["basketbal"]  = (6.5,  "Basketbal",  "\U0001F3C0"),
        ["tenis"]      = (7.3,  "Tenis",      "\U0001F3BE"),
        ["joga"]       = (3.0,  "Jóga",       "\U0001F9D8"),
        ["chuze"]      = (3.5,  "Chůze",      "\U0001F6B6"),
        ["stretching"] = (2.3,  "Stretching", "\U0001F938"),
    };

    public static double GetMet(string sport) =>
        _sporty.TryGetValue(sport?.ToLower() ?? "", out var info) ? info.Met : 5.0;

    public static string GetNazev(string sport) =>
        Loc.SportName(sport ?? "");

    public static string GetIkona(string sport) =>
        _sporty.TryGetValue(sport?.ToLower() ?? "", out var info) ? info.Ikona : "\U0001F4AA";

    /// <summary>
    /// Vypočítá spálené kalorie: MET × váha (kg) × čas (hodiny)
    /// </summary>
    public static int SpocitejKalorie(string sport, double vahaKg, TimeSpan cas)
    {
        if (vahaKg <= 0) vahaKg = 70;
        double met = GetMet(sport);
        return (int)Math.Round(met * vahaKg * cas.TotalHours);
    }
}
