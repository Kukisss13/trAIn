using TreninkovyPlanovac.Helpers;

namespace TreninkovyPlanovac;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Loc.Instance.RefreshBindings();
	}

	private async void OnTreninkyClicked(object? sender, EventArgs e)
	{
		if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
		await Shell.Current.GoToAsync("treninky");
	}

	private async void OnZacitWorkoutClicked(object? sender, EventArgs e)
	{
		if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
		await Shell.Current.GoToAsync("zacitworkout");
	}

	private async void OnMojeCestaClicked(object? sender, EventArgs e)
	{
		if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
		await Shell.Current.GoToAsync("mojecesta");
	}

	private async void OnNastaveniClicked(object? sender, EventArgs e)
	{
		if (sender is View v) await AnimaceHelper.AnimovatKlik(v);
		await Shell.Current.GoToAsync("nastaveni");
	}

}
