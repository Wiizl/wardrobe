using Microsoft.Maui.Controls.Shapes;

namespace Wardrobe.Controls;

/// <summary>
/// In-page modal bottom sheet used instead of system pickers and alert dialogs.
/// Place it as the last child of the page's root Grid (spanning all rows/columns).
/// </summary>
public sealed class BottomSheet : ContentView
{
	readonly BoxView _scrim;
	readonly Border _card;
	readonly Label _title;
	readonly VerticalStackLayout _body = new() { Spacing = 12 };
	Action? _cancel;
	int _version;

	public BottomSheet()
	{
		IsVisible = false;

		_scrim = new BoxView().SetColor(BoxView.ColorProperty, Theme.Scrim);
		var scrimTap = new TapGestureRecognizer();
		scrimTap.Tapped += (_, _) => _cancel?.Invoke();
		_scrim.GestureRecognizers.Add(scrimTap);

		var handle = new Border
		{
			WidthRequest = 40,
			HeightRequest = 5,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 3 },
			HorizontalOptions = LayoutOptions.Center,
		}.SetColor(BackgroundColorProperty, Theme.Outline);

		_title = new Label { FontFamily = Theme.FontSemibold, FontSize = 20 };

		_card = new Border
		{
			VerticalOptions = LayoutOptions.End,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(28, 28, 0, 0) },
			Padding = new Thickness(20, 12, 20, 28),
			Content = new VerticalStackLayout { Spacing = 16, Children = { handle, _title, _body } },
		}.SetColor(BackgroundColorProperty, Theme.Surface);
		// Swallow taps on the card so they don't reach the scrim underneath.
		_card.GestureRecognizers.Add(new TapGestureRecognizer());

		Content = new Grid { Children = { _scrim, _card } };
	}

	/// <summary>Call from the page's OnBackButtonPressed. Returns true if the sheet consumed the back press.</summary>
	public bool HandleBack()
	{
		if (!IsVisible)
			return false;
		_cancel?.Invoke();
		return true;
	}

	/// <summary>Shows a list of options; returns the chosen index or null when dismissed.</summary>
	public Task<int?> ShowOptionsAsync(string title, IReadOnlyList<string> options, int selectedIndex)
	{
		var tcs = new TaskCompletionSource<int?>();
		int v = Prepare(title);
		_cancel = () => Finish(tcs, null, v);

		var list = new VerticalStackLayout { Spacing = 2 };
		View? selectedRow = null;
		for (int i = 0; i < options.Count; i++)
		{
			int index = i;
			bool selected = i == selectedIndex;

			var label = new Label
			{
				Text = options[i],
				FontSize = 16,
				FontFamily = selected ? Theme.FontSemibold : Theme.FontRegular,
				VerticalOptions = LayoutOptions.Center,
			}.SetColor(Label.TextColorProperty, selected ? Theme.Accent : Theme.TextPrimary);

			var grid = new Grid { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) } };
			grid.Add(label, 0);
			if (selected)
				grid.Add(Icons.Create(Icons.Check, Theme.Accent, size: 20), 1);

			var row = new Border
			{
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 12 },
				Padding = new Thickness(14, 13),
				Content = grid,
			};
			if (selected)
			{
				row.SetColor(BackgroundColorProperty, Theme.AccentSoft);
				selectedRow = row;
			}
			else
			{
				row.BackgroundColor = Colors.Transparent;
			}
			row.OnTapped(() => Finish(tcs, index, v));
			list.Children.Add(row);
		}

		var scroll = new ScrollView
		{
			Content = list,
			MaximumHeightRequest = Math.Max(260, (Height > 0 ? Height : 800) * 0.6),
		};
		_body.Children.Add(scroll);

		_ = OpenAsync(async () =>
		{
			if (selectedRow is not null)
				await scroll.ScrollToAsync(selectedRow, ScrollToPosition.Center, false);
		});
		return tcs.Task;
	}

	/// <summary>Shows a message with Cancel/confirm buttons; returns true when confirmed.</summary>
	public Task<bool> ShowConfirmAsync(string title, string message, string confirmText, bool destructive = false)
	{
		var tcs = new TaskCompletionSource<bool>();
		int v = Prepare(title);
		_cancel = () => Finish(tcs, false, v);

		_body.Children.Add(MessageLabel(message));

		var cancel = new Button { Text = "Cancel", Style = ViewHelpers.Style("TonalButton") };
		cancel.Clicked += (_, _) => Finish(tcs, false, v);
		var confirm = new Button { Text = confirmText };
		if (destructive)
		{
			confirm.SetColor(BackgroundColorProperty, Theme.Danger);
			confirm.TextColor = Colors.White;
		}
		confirm.Clicked += (_, _) => Finish(tcs, true, v);

		var buttons = new Grid { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Star) }, ColumnSpacing = 12, Margin = new Thickness(0, 8, 0, 0) };
		buttons.Add(cancel, 0);
		buttons.Add(confirm, 1);
		_body.Children.Add(buttons);

		_ = OpenAsync();
		return tcs.Task;
	}

	/// <summary>Shows an informational message with a single OK button.</summary>
	public Task ShowMessageAsync(string title, string message)
	{
		var tcs = new TaskCompletionSource<bool>();
		int v = Prepare(title);
		_cancel = () => Finish(tcs, true, v);

		_body.Children.Add(MessageLabel(message));
		var ok = new Button { Text = "OK", Margin = new Thickness(0, 8, 0, 0) };
		ok.Clicked += (_, _) => Finish(tcs, true, v);
		_body.Children.Add(ok);

		_ = OpenAsync();
		return tcs.Task;
	}

	static Label MessageLabel(string message) =>
		new Label { Text = message, FontSize = 15 }.SetColor(Label.TextColorProperty, Theme.TextSecondary);

	/// <summary>Resets the sheet for new content and returns its version number.</summary>
	int Prepare(string title)
	{
		var previous = _cancel;
		_cancel = null;
		_version++;
		previous?.Invoke(); // resolve a still-open sheet; its version is stale so it won't close us
		_body.Children.Clear();
		_title.Text = title;
		_title.IsVisible = !string.IsNullOrEmpty(title);
		return _version;
	}

	void Finish<T>(TaskCompletionSource<T> tcs, T result, int version)
	{
		if (!tcs.TrySetResult(result) || version != _version)
			return;
		_cancel = null;
		_ = CloseAsync(version);
	}

	async Task OpenAsync(Func<Task>? afterOpen = null)
	{
		IsVisible = true;
		_scrim.Opacity = 0;
		_card.TranslationY = Height > 0 ? Height : 1000;
		await Task.WhenAll(
			_scrim.FadeToAsync(1, 180),
			_card.TranslateToAsync(0, 0, 260, Easing.CubicOut));
		if (afterOpen is not null)
			await afterOpen();
	}

	async Task CloseAsync(int version)
	{
		await Task.WhenAll(
			_scrim.FadeToAsync(0, 160),
			_card.TranslateToAsync(0, _card.Height + 40, 200, Easing.CubicIn));

		// A new sheet may have been opened while this one was closing.
		if (version != _version)
			return;
		IsVisible = false;
		_body.Children.Clear();
	}
}
