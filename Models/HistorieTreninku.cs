using SQLite;

namespace TreninkovyPlanovac.Models;

public class HistorieTreninku
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int TreninkovyPlanId { get; set; }

    public string NazevTreninku { get; set; } = string.Empty;

    public DateTime DatumCviceni { get; set; }

    public double CasMinuty { get; set; }

    public double CelkovaVahaKg { get; set; }

    public double CelkovaVzdalenostKm { get; set; }

    public double SpaleneKalorie { get; set; }

    public string TypSportu { get; set; } = string.Empty;
}
