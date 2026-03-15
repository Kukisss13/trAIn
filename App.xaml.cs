using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac;

public partial class App : Application
{
	private readonly ThemeService _theme;

	public App(SeedDataService seedService, ThemeService theme)
	{
		// Load saved language before InitializeComponent
		Loc.Instance.LoadSaved();

		InitializeComponent();
		_theme = theme;
		// Spustíme seed na pozadí hned při startu
		Task.Run(async () => await seedService.SeedAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		// Aplikuj motiv po vytvoření okna (resources jsou dostupné)
		_theme.AplikujMotiv();

#if WINDOWS
		window.HandlerChanged += (s, e) =>
		{
			var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
			if (nativeWindow?.Content is Microsoft.UI.Xaml.FrameworkElement root)
			{
				root.KeyDown += (sender, args) =>
				{
					if (args.Key == global::Windows.System.VirtualKey.F11)
						ToggleFullscreen(nativeWindow);
				};
			}
		};
#endif

		return window;
	}

#if WINDOWS
	private static void ToggleFullscreen(Microsoft.UI.Xaml.Window nativeWindow)
	{
		var appWindow = nativeWindow.AppWindow;
		if (appWindow.Presenter is Microsoft.UI.Windowing.FullScreenPresenter)
			appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Overlapped);
		else
			appWindow.SetPresenter(Microsoft.UI.Windowing.FullScreenPresenter.Create());
	}
#endif
}
