using Wardrobe.Models;
using Wardrobe.Services;

namespace Wardrobe.Pages;

public partial class ShelfPage : ContentPage
{
	readonly WardrobeStore _store;
	readonly string _wardrobeId;
	readonly string _shelfId;
	readonly string _wardrobeName;

	public string WardrobeId => _wardrobeId;
	public string ShelfId => _shelfId;

	public ShelfPage(WardrobeStore store, string wardrobeId, string shelfId)
	{
		_store = store;
		_wardrobeId = wardrobeId;
		_shelfId = shelfId;
		InitializeComponent();

		var wardrobe = store.GetWardrobe(wardrobeId);
		var shelf = wardrobe?.FindShelf(shelfId);
		_wardrobeName = wardrobe?.Name ?? "";
		Header.Title = shelf is null ? "Shelf" : $"Shelf {shelf.Name}";
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		var items = _store.GetItems(_wardrobeId, _shelfId);
		ItemsView.ItemsSource = items;
		Header.Subtitle = $"{_wardrobeName} · {items.Count} {(items.Count == 1 ? "item" : "items")}";
	}

	async void OnItemTapped(object? sender, TappedEventArgs e)
	{
		if ((sender as BindableObject)?.BindingContext is not ClothingItem item)
			return;
		await Navigation.PushAsync(new ItemEditPage(_store, item, item.WardrobeId, item.ShelfId));
	}

	async void OnAddClicked(object? sender, EventArgs e)
	{
		await Navigation.PushAsync(new ItemEditPage(_store, null, _wardrobeId, _shelfId));
	}
}
