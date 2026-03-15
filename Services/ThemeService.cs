namespace TreninkovyPlanovac.Services;

public class ThemeService
{
    private const string ThemeKey = "app_theme";

    // true = tmavý, false = světlý
    public bool JeTmavy
    {
        get => Preferences.Get(ThemeKey, true);
        set
        {
            Preferences.Set(ThemeKey, value);
            AplikujMotiv();
        }
    }

    public void AplikujMotiv()
    {
        var resources = Application.Current?.Resources;
        if (resources == null) return;

        if (JeTmavy)
        {
            resources["DarkBg"] = Color.FromArgb("#0F0F1A");
            resources["CardBg"] = Color.FromArgb("#1A1A2E");
            resources["CardBgLight"] = Color.FromArgb("#222240");
            resources["TextPrimary"] = Colors.White;
            resources["TextSecondary"] = Color.FromArgb("#ACACAC");
            resources["TextTertiary"] = Color.FromArgb("#919191");
            resources["IconBg"] = Color.FromArgb("#2A2A4A");
            resources["SeparatorColor"] = Color.FromArgb("#404040");
            resources["InputBg"] = Color.FromArgb("#1A1A2E");
        }
        else
        {
            resources["DarkBg"] = Color.FromArgb("#F2F2F7");
            resources["CardBg"] = Colors.White;
            resources["CardBgLight"] = Color.FromArgb("#E8E8F0");
            resources["TextPrimary"] = Color.FromArgb("#1A1A2E");
            resources["TextSecondary"] = Color.FromArgb("#6E6E6E");
            resources["TextTertiary"] = Color.FromArgb("#919191");
            resources["IconBg"] = Color.FromArgb("#E8E8F0");
            resources["SeparatorColor"] = Color.FromArgb("#D0D0D0");
            resources["InputBg"] = Color.FromArgb("#F0F0F5");
        }
    }
}
