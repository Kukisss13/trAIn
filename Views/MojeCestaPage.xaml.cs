using TreninkovyPlanovac.Helpers;
using TreninkovyPlanovac.Services;

namespace TreninkovyPlanovac.Views;

public partial class MojeCestaPage : ContentPage
{
	private readonly DatabaseService _db;

	public MojeCestaPage(DatabaseService db)
	{
		InitializeComponent();
		_db = db;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		Loc.Instance.RefreshBindings();
		await NactiStatistiky();
	}

	private async Task NactiStatistiky()
	{
		var historie = await _db.GetHistoriiAsync();

		int pocet = historie.Count;
		double vaha = historie.Sum(h => h.CelkovaVahaKg);
		double minuty = historie.Sum(h => h.CasMinuty);
		double km = historie.Sum(h => h.CelkovaVzdalenostKm);
		double kalorie = historie.Sum(h => h.SpaleneKalorie);

		WorkoutyLabel.Text = pocet.ToString();
		KilogramyLabel.Text = vaha >= 1000 ? $"{Math.Round(vaha / 1000, 1)} t" : $"{Math.Round(vaha)} kg";
		MinutyLabel.Text = minuty >= 60 ? $"{Math.Round(minuty / 60, 1)} h" : $"{Math.Round(minuty)} min";
		KilometryLabel.Text = $"{Math.Round(km, 1)} km";
		KalorieLabel.Text = kalorie >= 1000 ? $"{Math.Round(kalorie / 1000, 1)} Mcal" : $"{Math.Round(kalorie)} kcal";

		// Počet tréninků podle sportu
		SportyStatsLayout.Children.Clear();

		var skupiny = historie
			.Where(h => !string.IsNullOrEmpty(h.TypSportu))
			.GroupBy(h => h.TypSportu.ToLower())
			.OrderByDescending(g => g.Count())
			.ToList();

		foreach (var skupina in skupiny)
		{
			string sport = skupina.Key;
			int count = skupina.Count();
			string ikona = SportMet.GetIkona(sport);
			string nazev = SportMet.GetNazev(sport);
			double sportMin = skupina.Sum(h => h.CasMinuty);
			double sportKcal = skupina.Sum(h => h.SpaleneKalorie);

			var frame = new Frame
			{
				BackgroundColor = (Color)Application.Current!.Resources["CardBg"],
				CornerRadius = 16,
				Padding = new Thickness(16, 14),
				BorderColor = Colors.Transparent,
				HasShadow = false
			};

			var grid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition(new GridLength(50)),
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Auto)
				}
			};

			var ikonaLabel = new Label
			{
				Text = ikona,
				FontSize = 30,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			var infoStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 2 };
			infoStack.Children.Add(new Label
			{
				Text = nazev,
				FontSize = 16,
				FontAttributes = FontAttributes.Bold,
				TextColor = (Color)Application.Current.Resources["TextPrimary"]
			});
			string casText = sportMin >= 60 ? $"{Math.Round(sportMin / 60, 1)} h" : $"{Math.Round(sportMin)} min";
			infoStack.Children.Add(new Label
			{
				Text = $"{casText}  •  {Math.Round(sportKcal)} kcal",
				FontSize = 12,
				TextColor = (Color)Application.Current.Resources["TextTertiary"]
			});

			var countStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
			countStack.Children.Add(new Label
			{
				Text = count.ToString(),
				FontSize = 28,
				FontAttributes = FontAttributes.Bold,
				TextColor = (Color)Application.Current.Resources["Primary"],
				HorizontalOptions = LayoutOptions.Center
			});
			countStack.Children.Add(new Label
			{
				Text = Loc.WorkoutCount(count),
				FontSize = 11,
				TextColor = (Color)Application.Current.Resources["TextTertiary"],
				HorizontalOptions = LayoutOptions.Center
			});

			grid.Add(ikonaLabel, 0);
			grid.Add(infoStack, 1);
			grid.Add(countStack, 2);
			frame.Content = grid;

			SportyStatsLayout.Children.Add(frame);
		}

		if (skupiny.Count == 0)
		{
			SportyStatsLayout.Children.Add(new Label
			{
				Text = Loc.T("NoWorkoutsYet"),
				FontSize = 14,
				TextColor = (Color)Application.Current!.Resources["TextTertiary"],
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 10)
			});
		}
	}

	private async void OnZpetClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
}
