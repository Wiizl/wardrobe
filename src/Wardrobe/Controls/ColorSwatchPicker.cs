using Microsoft.Maui.Controls.Shapes;
using Wardrobe.Models;

namespace Wardrobe.Controls;

/// <summary>Grid of round color swatches. Tapping the selected swatch clears the selection.</summary>
public sealed class ColorSwatchPicker : ContentView
{
	const int Columns = 7;
	readonly Grid _grid = new() { RowSpacing = 10 };
	readonly Dictionary<ClothingColor, (Border Ring, View Check)> _swatches = [];
	ClothingColor? _selected;

	public event EventHandler? SelectionChanged;

	public ColorSwatchPicker()
	{
		for (int c = 0; c < Columns; c++)
			_grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

		for (int i = 0; i < ClothingColors.All.Count; i++)
		{
			var color = ClothingColors.All[i];
			var swatchColor = ClothingColors.Swatch(color);
			var fill = new Ellipse
			{
				Fill = swatchColor,
				StrokeThickness = 1,
				WidthRequest = 32,
				HeightRequest = 32,
			}.SetColor(Shape.StrokeProperty, Theme.Outline);

			var checkColor = swatchColor.GetLuminosity() > 0.6f ? Colors.Black : Colors.White;
			var check = Icons.Create(Icons.Check, checkColor, size: 18, thickness: 2.6);

			var ring = new Border
			{
				StrokeShape = new Ellipse(),
				StrokeThickness = 2.5,
				Padding = 3,
				HorizontalOptions = LayoutOptions.Center,
				Content = new Grid { Children = { fill, check } },
			};
			ring.OnTapped(() => Toggle(color));
			SemanticProperties.SetDescription(ring, ClothingColors.DisplayName(color));

			_swatches[color] = (ring, check);
			if (i % Columns == 0)
				_grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			_grid.Add(ring, i % Columns, i / Columns);
		}

		Content = _grid;
		Update();
	}

	public ClothingColor? SelectedColor
	{
		get => _selected;
		set
		{
			_selected = value;
			Update();
		}
	}

	void Toggle(ClothingColor color)
	{
		SelectedColor = _selected == color ? null : color;
		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	void Update()
	{
		foreach (var (color, (ring, check)) in _swatches)
		{
			bool selected = color == _selected;
			check.IsVisible = selected;
			if (selected)
				ring.SetColor(Border.StrokeProperty, Theme.Accent);
			else
				ring.Stroke = Colors.Transparent;
		}
	}
}
