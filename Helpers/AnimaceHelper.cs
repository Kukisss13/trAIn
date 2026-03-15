using Plugin.Maui.Audio;

namespace TreninkovyPlanovac.Helpers;

/// <summary>
/// Helper pro animace kliknutí a zvukový feedback
/// </summary>
public static class AnimaceHelper
{
    private static IAudioPlayer? _clickPlayer;
    private static bool _audioNacten;

    /// <summary>
    /// Přehraje krátkou animaci stisknutí (scale down/up) + tichý click zvuk
    /// </summary>
    public static async Task AnimovatKlik(View element)
    {
        // Zvuk
        PrehratClick();

        // Animace: zmenšit → zvětšit zpět
        await element.ScaleTo(0.92, 80, Easing.CubicIn);
        await element.ScaleTo(1.0, 80, Easing.CubicOut);
    }

    /// <summary>
    /// Přehraje click zvuk (potichu)
    /// </summary>
    private static async void PrehratClick()
    {
        try
        {
            if (!_audioNacten)
            {
                _audioNacten = true;
                var audioManager = AudioManager.Current;
                var stream = await FileSystem.OpenAppPackageFileAsync("click.wav");
                _clickPlayer = audioManager.CreatePlayer(stream);
                _clickPlayer.Volume = 0.3; // Potichu
            }

            if (_clickPlayer != null)
            {
                // Pokud ještě hraje, přetočit na začátek
                if (_clickPlayer.IsPlaying)
                    _clickPlayer.Stop();
                _clickPlayer.Seek(0);
                _clickPlayer.Play();
            }
        }
        catch
        {
            // Zvuk není kritický, pokud selže, ignorujeme
        }
    }
}
