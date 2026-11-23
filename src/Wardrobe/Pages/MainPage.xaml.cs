using Wardrobe.Controls;
using Wardrobe.Services;

namespace Wardrobe.Pages;

public partial class MainPage : ContentPage
{
	readonly WardrobeStore _store;

	public MainPage(WardrobeStore store)
	{
		_store = store;
		InitializeComponent();

		AddButtonContent.Children.Add(Icons.Create(Icons.Plus, Theme.OnAccent));
		AddButtonContent.Children.Add(new Label
		{
			Text = "Add item",
			FontFamily = Theme.FontSemibold,
			FontSize = 16,
			VerticalOptions = LayoutOptions.Center,
		}.SetColor(Label.TextColorProperty, Theme.OnAccent));
		AddButton.OnTapped(OnAddClicked);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		try
		{
			await _store.InitializeAsync();
		}
		catch (Exception ex)
		{
			await Sheet.ShowMessageAsync("Couldn't load data", ex.Message);
			return;
		}
		Refresh();
	}

	protected override bool OnBackButtonPressed() => Sheet.HandleBack() || base.OnBackButtonPressed();

	void Refresh()
	{
		var total = _store.Data.Items.Count;
		Header.Subtitle = total == 1 ? "1 item in total" : $"{total} items in total";

		var selectedId = _store.SelectedWardrobeId;
		WardrobeChips.SetItems(_store.Wardrobes.Select(w => (w.Id, w.Name)), selectedId);
		ShowWardrobe(selectedId);
	}

	void ShowWardrobe(string wardrobeId)
	{
		var wardrobe = _store.GetWardrobe(wardrobeId);
		var counts = _store.GetShelfCounts(wardrobeId);
		var items = counts.Values.Sum();

		WardrobeName.Text = wardrobe?.Name ?? "";
		WardrobeInfo.Text = wardrobe is null
			? ""
			: $"{wardrobe.Shelves.Count} shelves · {items} {(items == 1 ? "item" : "items")}";
		LayoutView.SetWardrobe(wardrobe, counts);
	}

	void OnWardrobeChanged(object? sender, string wardrobeId)
	{
		_store.SelectedWardrobeId = wardrobeId;
		ShowWardrobe(wardrobeId);
	}

	async void OnShelfTapped(object? sender, string shelfId)
	{
		await Navigation.PushAsync(new ShelfPage(_store, _store.SelectedWardrobeId, shelfId));
	}

	async void OnAddClicked()
	{
		if (_store.Wardrobes.Count == 0)
			return;
		await Navigation.PushAsync(new ItemEditPage(_store, null, _store.SelectedWardrobeId, null));
	}
}
