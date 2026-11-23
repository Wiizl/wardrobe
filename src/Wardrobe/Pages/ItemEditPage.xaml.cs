using Wardrobe.Models;
using Wardrobe.Services;

namespace Wardrobe.Pages;

/// <summary>Add a new clothing item, or view/edit an existing one (incl. moving it to another shelf).</summary>
public partial class ItemEditPage : ContentPage
{
	readonly WardrobeStore _store;
	readonly ClothingItem _item;      // working copy
	readonly bool _isNew;
	readonly List<string> _types;
	string? _pendingImagePath;        // resized image not yet saved
	bool _busy;

	public ItemEditPage(WardrobeStore store, ClothingItem? existing, string wardrobeId, string? shelfId)
	{
		_store = store;
		_isNew = existing is null;
		_item = existing?.Clone() ?? new ClothingItem { WardrobeId = wardrobeId, ShelfId = shelfId ?? "" };

		InitializeComponent();

		Header.Title = _isNew ? "New item" : "Edit item";
		Header.ActionText = _isNew ? "" : "Delete";

		NameEntry.Text = _item.Name;

		_types = ClothingTypes.All.ToList();
		if (!string.IsNullOrWhiteSpace(_item.Type) && !_types.Contains(_item.Type))
			_types.Insert(0, _item.Type);
		TypeField.Text = _item.Type;

		ColorPicker.SelectedColor = _item.Color;
		UpdateColorName();

		AtBottomSwitch.IsToggled = _item.IsAtBottom;

		WardrobeChips.SetItems(store.Wardrobes.Select(w => (w.Id, w.Name)), _item.WardrobeId);
		WardrobeChips.IsVisible = store.Wardrobes.Count > 1;

		ShowImage(_item.ImagePath);
		ShowWardrobe();
	}

	protected override bool OnBackButtonPressed() => Sheet.HandleBack() || base.OnBackButtonPressed();

	void ShowImage(string? path)
	{
		var has = !string.IsNullOrEmpty(path) && File.Exists(path);
		ItemImage.Source = has ? ImageSource.FromFile(path) : null;
		NoImage.IsVisible = !has;
	}

	void ShowWardrobe()
	{
		var wardrobe = _store.GetWardrobe(_item.WardrobeId);
		LayoutView.SetWardrobe(wardrobe, _store.GetShelfCounts(_item.WardrobeId));
		LayoutView.SelectedShelfId = _item.ShelfId;

		var shelf = wardrobe?.FindShelf(_item.ShelfId);
		ShelfLabel.Text = shelf is null
			? "Tap the shelf where this item goes"
			: $"Shelf {shelf.Name} · tap another shelf to move it";
	}

	async void OnTypeTapped(object? sender, EventArgs e)
	{
		var index = await Sheet.ShowOptionsAsync("Type", _types, _types.IndexOf(TypeField.Text));
		if (index is { } i)
			TypeField.Text = _types[i];
	}

	void OnColorChanged(object? sender, EventArgs e) => UpdateColorName();

	void OnAtBottomRowTapped(object? sender, TappedEventArgs e) => AtBottomSwitch.Toggle();

	void UpdateColorName() =>
		ColorName.Text = ColorPicker.SelectedColor is { } c ? ClothingColors.DisplayName(c) : "None";

	void OnWardrobeChanged(object? sender, string wardrobeId)
	{
		if (wardrobeId == _item.WardrobeId)
			return;
		_item.WardrobeId = wardrobeId;
		_item.ShelfId = "";   // shelves belong to a specific wardrobe
		ShowWardrobe();
	}

	void OnShelfTapped(object? sender, string shelfId)
	{
		_item.ShelfId = shelfId;
		ShowWardrobe();
	}

	async void OnPickPhotoClicked(object? sender, EventArgs e)
	{
		try
		{
			var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
			{
				Title = "Select a photo",
				SelectionLimit = 1,
			});
			await ImportAsync(results?.FirstOrDefault());
		}
		catch (Exception ex)
		{
			await Sheet.ShowMessageAsync("Couldn't pick photo", ex.Message);
		}
	}

	async void OnTakePhotoClicked(object? sender, EventArgs e)
	{
		try
		{
			if (!MediaPicker.Default.IsCaptureSupported)
			{
				await Sheet.ShowMessageAsync("No camera", "This device has no camera.");
				return;
			}
			if (await Permissions.RequestAsync<Permissions.Camera>() != PermissionStatus.Granted)
				return;

			await ImportAsync(await MediaPicker.Default.CapturePhotoAsync());
		}
		catch (Exception ex)
		{
			await Sheet.ShowMessageAsync("Couldn't take photo", ex.Message);
		}
	}

	async Task ImportAsync(FileResult? file)
	{
		if (file is null)
			return;

		SetBusy(true);
		try
		{
			await using var stream = await file.OpenReadAsync();
			var path = await _store.ImportPendingImageAsync(stream);
			DeletePending();
			_pendingImagePath = path;
			ShowImage(path);
		}
		finally
		{
			SetBusy(false);
		}
	}

	async void OnSaveClicked(object? sender, EventArgs e)
	{
		if (_busy)
			return;

		var name = NameEntry.Text?.Trim() ?? "";
		if (name.Length == 0)
		{
			await Sheet.ShowMessageAsync("Name missing", "Give the item a name so you can find it later.");
			return;
		}
		if (_store.GetWardrobe(_item.WardrobeId)?.FindShelf(_item.ShelfId) is null)
		{
			await Sheet.ShowMessageAsync("Shelf missing", "Tap the shelf where this item goes.");
			return;
		}

		_item.Name = name;
		_item.Color = ColorPicker.SelectedColor;
		_item.Type = TypeField.Text;
		_item.IsAtBottom = AtBottomSwitch.IsToggled;

		SetBusy(true);
		try
		{
			await _store.SaveItemAsync(_item, _pendingImagePath);
			_pendingImagePath = null;
			ShowSavedLocationOnReturn();
			await Navigation.PopAsync();
		}
		catch (Exception ex)
		{
			await Sheet.ShowMessageAsync("Couldn't save", ex.Message);
		}
		finally
		{
			SetBusy(false);
		}
	}

	/// <summary>
	/// If this page was opened from a shelf page and the item ended up on a different shelf,
	/// swap that shelf page for the one the item was saved on, so going back shows the item.
	/// </summary>
	void ShowSavedLocationOnReturn()
	{
		_store.SelectedWardrobeId = _item.WardrobeId;

		var stack = Navigation.NavigationStack;
		var index = stack.ToList().IndexOf(this);
		if (index < 1 || stack[index - 1] is not ShelfPage previous)
			return;
		if (previous.WardrobeId == _item.WardrobeId && previous.ShelfId == _item.ShelfId)
			return;

		Navigation.InsertPageBefore(new ShelfPage(_store, _item.WardrobeId, _item.ShelfId), this);
		Navigation.RemovePage(previous);
	}

	async void OnDeleteClicked(object? sender, EventArgs e)
	{
		if (_busy || _isNew)
			return;
		if (!await Sheet.ShowConfirmAsync("Delete item?", $"\"{_item.Name}\" and its photo will be removed.", "Delete", destructive: true))
			return;

		SetBusy(true);
		try
		{
			await _store.DeleteItemAsync(_item.Id);
			DeletePending();
			await Navigation.PopAsync();
		}
		finally
		{
			SetBusy(false);
		}
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);
		// Leaving without saving: drop the unsaved resized photo.
		if (!Navigation.NavigationStack.Contains(this))
			DeletePending();
	}

	void DeletePending()
	{
		if (_pendingImagePath is null)
			return;
		try { File.Delete(_pendingImagePath); } catch (IOException) { }
		_pendingImagePath = null;
	}

	void SetBusy(bool busy)
	{
		_busy = busy;
		Busy.IsVisible = busy;
	}
}
