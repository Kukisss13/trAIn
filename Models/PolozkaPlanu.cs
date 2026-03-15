using SQLite;

namespace TreninkovyPlanovac.Models;

public class PolozkaPlanu
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TreninkovyPlanId { get; set; }

    [Indexed]
    public int CviceniId { get; set; }

    public string NazevCviku { get; set; } = string.Empty;  // pro případ vlastního názvu
    public int Serie { get; set; }           // počet sérií
    public string OpakovaniDetail { get; set; } = string.Empty;  // "12,10,8,6" — opakování per série
    public double Vaha { get; set; }         // váha v kg
    public int Poradi { get; set; }          // pořadí cviku v tréninku
    public int Pauza { get; set; }           // přibližná pauza v sekundách
    public string Poznamka { get; set; } = string.Empty;
}
