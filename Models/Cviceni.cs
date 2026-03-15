using SQLite;

namespace TreninkovyPlanovac.Models;

public class Cviceni
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nazev { get; set; } = string.Empty;

    public string Popis { get; set; } = string.Empty;

    public string Kategorie { get; set; } = string.Empty; // "Hrudník", "Záda", "Nohy", "Ramena", "Ruce", "Core", "Kardio"

    [Ignore]
    public string KategorieObrazek => Kategorie switch
    {
        "Hrudník" => "hrudnik.jpg",
        _ => ""
    };

    [Ignore]
    public string KategorieIkona => Kategorie switch
    {
        "Hrudník"  => "🏋️",
        "Záda"     => "🔙",
        "Nohy"     => "🦵",
        "Ramena"   => "🤸",
        "Ruce"     => "💪",
        "Core"     => "🔥",
        "Kardio"   => "❤️",
        _          => "⚡"
    };

    [Ignore]
    public Color KategorieBarva => Kategorie switch
    {
        "Hrudník"  => Color.FromArgb("#7C4DFF"),
        "Záda"     => Color.FromArgb("#00BCD4"),
        "Nohy"     => Color.FromArgb("#FF6B6B"),
        "Ramena"   => Color.FromArgb("#FFB300"),
        "Ruce"     => Color.FromArgb("#66BB6A"),
        "Core"     => Color.FromArgb("#FF7043"),
        "Kardio"   => Color.FromArgb("#EC407A"),
        _          => Color.FromArgb("#546E7A")
    };
}
