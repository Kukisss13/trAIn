using SQLite;

namespace TreninkovyPlanovac.Models;

public class ChatZprava
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty; // "user" nebo "assistant"
    public string Obsah { get; set; } = string.Empty;
    public DateTime Cas { get; set; } = DateTime.Now;

    [Ignore]
    public bool JeUser => Role == "user";
    [Ignore]
    public bool JeAssistant => Role == "assistant";
}
