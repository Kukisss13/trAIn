namespace TreninkovyPlanovac.Models;

/// <summary>
/// Pomocný model pro zobrazení položky tréninku s názvem cviku
/// </summary>
public class PolozkaSCvikem
{
    public int CviceniId { get; set; }
    public string NazevCviku { get; set; } = string.Empty;
    public int Serie { get; set; } = 3;
    public int Opakovani { get; set; } = 10;
    public double Vaha { get; set; }
    public int Pauza { get; set; } = 90;
    public string Poznamka { get; set; } = string.Empty;
    public int Poradi { get; set; }
}
