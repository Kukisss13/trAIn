using SQLite;

namespace TreninkovyPlanovac.Models;

public class TreninkovyPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nazev { get; set; } = string.Empty;

    public DateTime Datum { get; set; }

    public string Poznamka { get; set; } = string.Empty;

    public bool JeKoncept { get; set; } = true;
}
