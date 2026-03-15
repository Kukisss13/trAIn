using System.Text;
using System.Text.Json;
using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Models;

namespace TreninkovyPlanovac.Services;

public class ClaudeApiService
{
    private readonly HttpClient _http;
    private readonly DatabaseService _db;
    private const string GeminiModel = "gemini-2.0-flash-lite";
    private const string ApiKeyPref = "claude_api_key";

    public ClaudeApiService(HttpClient http, DatabaseService db)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(60);
        _db = db;
    }

    public string ApiKey
    {
        get => Preferences.Get(ApiKeyPref, "");
        set => Preferences.Set(ApiKeyPref, value);
    }

    public bool MaApiKlic => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<string> SestrojSystemPromptAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.T("AiSystemRole"));
        sb.AppendLine(Loc.T("AiSystemTone"));
        sb.AppendLine(Loc.T("AiSystemBrief"));
        sb.AppendLine();

        try
        {
            var uzivatel = await _db.GetNeboVytvorProfilAsync();
            if (!string.IsNullOrEmpty(uzivatel.Jmeno) || uzivatel.Vaha > 0)
            {
                sb.AppendLine(Loc.T("AiUserProfile"));
                if (!string.IsNullOrEmpty(uzivatel.Jmeno))
                    sb.AppendLine($"- {Loc.T("AiProfileName")}: {uzivatel.Jmeno}");
                if (uzivatel.Vek > 0)
                    sb.AppendLine($"- {Loc.T("AiProfileAge")}: {uzivatel.Vek}");
                if (!string.IsNullOrEmpty(uzivatel.Pohlavi))
                    sb.AppendLine($"- {Loc.T("AiProfileGender")}: {(uzivatel.Pohlavi == "muz" ? Loc.T("Male") : Loc.T("Female"))}");
                if (uzivatel.Vaha > 0)
                    sb.AppendLine($"- {Loc.T("AiProfileWeight")}: {uzivatel.Vaha} kg");
                if (uzivatel.Vyska > 0)
                    sb.AppendLine($"- {Loc.T("AiProfileHeight")}: {uzivatel.Vyska} cm");
                if (uzivatel.Vaha > 0 && uzivatel.Vyska > 0)
                {
                    var bmi = uzivatel.Vaha / Math.Pow(uzivatel.Vyska / 100.0, 2);
                    sb.AppendLine($"- BMI: {bmi:F1}");
                }
                if (!string.IsNullOrEmpty(uzivatel.Cil))
                    sb.AppendLine($"- {Loc.T("AiProfileGoal")}: {uzivatel.Cil}");
                if (!string.IsNullOrEmpty(uzivatel.UrovenAktivity))
                    sb.AppendLine($"- {Loc.T("AiProfileLevel")}: {uzivatel.UrovenAktivity}");
                sb.AppendLine();
            }

            var historie = await _db.GetHistoriiAsync();
            if (historie.Count > 0)
            {
                sb.AppendLine(Loc.T("AiRecentWorkouts"));
                foreach (var h in historie.Take(10))
                {
                    var parts = new List<string>();
                    if (h.CasMinuty > 0) parts.Add($"{h.CasMinuty:F0} min");
                    if (h.SpaleneKalorie > 0) parts.Add($"{h.SpaleneKalorie:F0} kcal");
                    if (h.CelkovaVahaKg > 0) parts.Add($"{h.CelkovaVahaKg:F0} kg");
                    if (h.CelkovaVzdalenostKm > 0) parts.Add($"{h.CelkovaVzdalenostKm:F1} km");
                    var detail = parts.Count > 0 ? $" ({string.Join(", ", parts)})" : "";
                    sb.AppendLine($"- {h.DatumCviceni:d.M.yyyy}: {h.NazevTreninku}{detail}");
                }
                sb.AppendLine();
            }
        }
        catch { }

        sb.AppendLine(Loc.T("AiSystemAdvice"));
        sb.AppendLine(Loc.T("AiSystemNoMedical"));

        return sb.ToString();
    }

    public async Task<string> PoslatZpravuAsync(List<ChatZprava> historie, string systemPrompt)
    {
        if (!MaApiKlic)
            return Loc.T("ErrNoApiKey");

        try
        {
            // Gemini používá "model" místo "assistant"
            var contents = historie.Select(z => new
            {
                role = z.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = z.Obsah } }
            }).ToList();

            var body = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents,
                generationConfig = new
                {
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(body);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={ApiKey}";

            HttpResponseMessage response = null!;
            string responseJson = "";
            const int maxPokusu = 3;

            for (int pokus = 1; pokus <= maxPokusu; pokus++)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                response = await _http.SendAsync(request);
                responseJson = await response.Content.ReadAsStringAsync();

                if ((int)response.StatusCode != 429 || pokus == maxPokusu)
                    break;

                await Task.Delay(pokus * 2000); // 2s, 4s
            }

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 400)
                    return Loc.T("ErrInvalidKey");
                if ((int)response.StatusCode == 429)
                    return Loc.T("ErrTooManyRequests");
                return string.Format(Loc.T("ErrApi"), (int)response.StatusCode);
            }

            using var doc = JsonDocument.Parse(responseJson);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? Loc.T("ErrEmptyResponse");
        }
        catch (TaskCanceledException)
        {
            return Loc.T("ErrTimeout");
        }
        catch (HttpRequestException)
        {
            return Loc.T("ErrConnection");
        }
        catch (Exception ex)
        {
            return string.Format(Loc.T("ErrUnexpected"), ex.Message);
        }
    }
}
