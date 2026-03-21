using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Plugin.Maui.Audio;
using TreninkovyPlanovac.Services;
using TreninkovyPlanovac.Views;

namespace TreninkovyPlanovac;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Audio
		builder.Services.AddSingleton(AudioManager.Current);

		// Služby
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<SeedDataService>();
		builder.Services.AddSingleton<ThemeService>();
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<ClaudeApiService>();

		// Stránky
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<CviceniPage>();
		builder.Services.AddTransient<NovyTreninkPage>();
		builder.Services.AddTransient<TreninkTabulkaPage>();
		builder.Services.AddTransient<VyberCvikuPage>();
		builder.Services.AddTransient<TreninkyPage>();
		builder.Services.AddTransient<DetailTreninkuPage>();
		builder.Services.AddTransient<NastaveniPage>();
		builder.Services.AddTransient<RegistracePage>();
		builder.Services.AddTransient<ZacitWorkoutPage>();
		builder.Services.AddTransient<MojeCestaPage>();
		builder.Services.AddTransient<SpustitTreninkPage>();
		builder.Services.AddTransient<SportTimerPage>();
		builder.Services.AddTransient<ChatPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// iOS: Safe Area na všech stránkách (notch / Dynamic Island)
#if IOS
		Microsoft.Maui.Handlers.PageHandler.Mapper.AppendToMapping("SafeArea", (handler, view) =>
		{
			if (view is ContentPage page)
				page.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(true);
		});
#endif

		return builder.Build();
	}
}
