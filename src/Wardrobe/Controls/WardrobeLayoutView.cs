using Microsoft.Maui.Controls.Shapes;
using Wardrobe.Models;

namespace Wardrobe.Controls;

/// <summary>
/// Draws a wardrobe's shelf layout and raises <see cref="ShelfTapped"/> when a shelf is tapped.
/// Used both for browsing (main page) and for choosing a shelf (item editor).
/// </summary>
public sealed class WardrobeLayoutView : ContentView
{
	/// <summary>Tall (narrow) layouts are capped at this height and centered instead of filling the width.</summary>
	const double MaxLayoutHeight = 460;

	readonly Grid _grid = new() { ColumnSpacing = 6, RowSpacing = 6 };
	readonly Border _frame;
	readonly Dictionary<string, (Border View, Span Number, Span Caption, int Count)> _shelfViews = [];

	WardrobeDefinition? _wardrobe;
	IReadOnlyDictionary<string, int> _counts = new Dictionary<string, int>();
	string? _selectedShelfId;

	public event EventHandler<string>? ShelfTapped;

	public WardrobeLayoutView()
	{
		Content = _frame = new Border
		{
			HorizontalOptions = LayoutOptions.Center,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 16 },
			Padding = 6,
			Content = _grid,
		}.SetColor(BackgroundColorProperty, Theme.ShelfFrame);
	}

	public string? SelectedShelfId
	{
		get => _selectedShelfId;
		set
		{
			_selectedShelfId = value;
			foreach (var (id, shelf) in _shelfViews)
				ApplyStyle(shelf.View, shelf.Number, shelf.Caption, shelf.Count, id == value);
		}
	}

	/// <summary>Shows <paramref name="wardrobe"/> with the item count (shelfId → count) on each shelf.</summary>
	public void SetWardrobe(WardrobeDefinition? wardrobe, IReadOnlyDictionary<string, int> counts)
	{
		_wardrobe = wardrobe;
		_counts = counts;
		Rebuild();
		UpdateSize(Width);
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		UpdateSize(width);
	}

	void UpdateSize(double width)
	{
		if (_wardrobe is null || width <= 0 || _wardrobe.AspectRatio <= 0)
			return;

		var height = Math.Round(width / _wardrobe.AspectRatio);
		var frameWidth = width;
		if (height > MaxLayoutHeight)
		{
			height = MaxLayoutHeight;
			frameWidth = Math.Round(height * _wardrobe.AspectRatio);
		}

		if (Math.Abs(HeightRequest - height) > 1)
			HeightRequest = height;
		if (Math.Abs(_frame.WidthRequest - frameWidth) > 1)
			_frame.WidthRequest = frameWidth;
	}

	void Rebuild()
	{
		_grid.Children.Clear();
		_grid.RowDefinitions.Clear();
		_grid.ColumnDefinitions.Clear();
		_shelfViews.Clear();

		if (_wardrobe is null)
			return;

		foreach (var w in _wardrobe.ColumnWeights)
			_grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(w, GridUnitType.Star)));
		foreach (var h in _wardrobe.RowWeights)
			_grid.RowDefinitions.Add(new RowDefinition(new GridLength(h, GridUnitType.Star)));

		foreach (var shelf in _wardrobe.Shelves)
		{
			var view = CreateShelfView(shelf);
			Grid.SetRow(view, shelf.Row);
			Grid.SetColumn(view, shelf.Column);
			Grid.SetRowSpan(view, Math.Max(1, shelf.RowSpan));
			Grid.SetColumnSpan(view, Math.Max(1, shelf.ColumnSpan));
			_grid.Children.Add(view);
		}
	}

	Border CreateShelfView(Shelf shelf)
	{
		_counts.TryGetValue(shelf.Id, out var count);

		var number = new Span { Text = count.ToString(), FontFamily = Theme.FontSemibold, FontSize = 16 };
		var caption = new Span { Text = count == 1 ? " item" : " items", FontFamily = Theme.FontRegular, FontSize = 12 };

		var border = new Border
		{
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 11 },
			Padding = 2,
			Content = new Label
			{
				FormattedText = new FormattedString { Spans = { number, caption } },
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center,
			},
		};
		ApplyStyle(border, number, caption, count, shelf.Id == _selectedShelfId);
		border.OnTapped(() => ShelfTapped?.Invoke(this, shelf.Id));
		SemanticProperties.SetDescription(border, $"Shelf {shelf.Name}, {count} items");

		_shelfViews[shelf.Id] = (border, number, caption, count);
		return border;
	}

	static void ApplyStyle(Border border, Span number, Span caption, int count, bool selected)
	{
		if (selected)
		{
			border.SetColor(BackgroundColorProperty, Theme.Accent);
			number.SetColor(Span.TextColorProperty, Theme.OnAccent);
			caption.SetColor(Span.TextColorProperty, Theme.OnAccent);
		}
		else
		{
			border.SetColor(BackgroundColorProperty, Theme.Shelf);
			number.SetColor(Span.TextColorProperty, count > 0 ? Theme.TextPrimary : Theme.TextSecondary);
			caption.SetColor(Span.TextColorProperty, Theme.TextSecondary);
		}
	}
}
