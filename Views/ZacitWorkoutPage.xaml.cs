using TreninkovyPlanovac.Helpers;

namespace TreninkovyPlanovac.Views;

public partial class ZacitWorkoutPage : ContentPage
{
	public ZacitWorkoutPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Loc.Instance.RefreshBindings();
	}

	private async void OnZpetClicked(object? sender, EventArgs e)
	{
		if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
		await Shell.Current.GoToAsync("..");
	}

	private async void OnSportClicked(object? sender, EventArgs e)
	{
		if (sender is not Frame frame) return;

		var tapGesture = frame.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
		var sport = tapGesture?.CommandParameter?.ToString() ?? "";

		await AnimaceHelper.AnimovatKlik(frame);

		if (sport == "posilovna")
		{
			await Shell.Current.GoToAsync("treninky");
			return;
		}

		await Shell.Current.GoToAsync($"sporttimer?sport={sport}");
	}
}
