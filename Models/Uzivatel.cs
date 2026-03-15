using SQLite;

namespace TreninkovyPlanovac.Models;

public class Uzivatel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Jmeno { get; set; } = string.Empty;
    public string MetodaPrihlaseni { get; set; } = string.Empty; // "email", "telefon", "apple"
    public DateTime DatumRegistrace { get; set; } = DateTime.Now;

    // Profil
    public double Vaha { get; set; }           // kg
    public double Vyska { get; set; }          // cm
    public int Vek { get; set; }
    public string Pohlavi { get; set; } = string.Empty;  // "muz", "zena"
    public string Cil { get; set; } = string.Empty;      // "nabrat", "zhubnout", "udrzovat", "kondice"
    public string UrovenAktivity { get; set; } = string.Empty; // "zacatecnik", "pokrocily", "expert"
}
