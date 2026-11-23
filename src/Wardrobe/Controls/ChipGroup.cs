using Microsoft.Maui.Controls.Shapes;

namespace Wardrobe.Controls;

/// <summary>Horizontally scrolling row of single-select pill chips (used as the wardrobe selector).</summary>
public sealed class ChipGroup : ContentView
{
	readonly HorizontalStackLayout _stack = new() { Spacing = 8 };
	readonly List<(string Key, Border Chip, Label Text)> _chips = [];
	string? _selectedKey;

	public event EventHandler<string>? SelectionChanged;

	public ChipGroup()
	{
		Content = new ScrollView
		{
			Orientation = ScrollOrientation.Horizontal,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
			Content = _stack,
		};
	}

	public void SetItems(IEnumerable<(string Key, string Text)> items, string? selectedKey)
	{
		_stack.Children.Clear();
		_chips.Clear();
		_selectedKey = selectedKey;

		foreach (var (key, text) in items)
		{
			var label = new Label
			{
				Text = text,
				FontFamily = Theme.FontSemibold,
				FontSize = 14,
				VerticalOptions = LayoutOptions.Center,
			};
			var chip = new Border
			{
				StrokeShape = new RoundRectangle { CornerRadius = 20 },
				Padding = new Thickness(18, 10),
				Content = label,
			};
			chip.OnTapped(() => Select(key));
			SemanticProperties.SetDescription(chip, text);

			_chips.Add((key, chip, label));
			_stack.Children.Add(chip);
			ApplyStyle(chip, label, key == selectedKey);
		}
	}

	public string? SelectedKey
	{
		get => _selectedKey;
		set
		{
			_selectedKey = value;
			foreach (var (key, chip, label) in _chips)
				ApplyStyle(chip, label, key == value);
		}
	}

	void Select(string key)
	{
		if (key == _selectedKey)
			return;
		SelectedKey = key;
		SelectionChanged?.Invoke(this, key);
	}

	static void ApplyStyle(Border chip, Label label, bool selected)
	{
		if (selected)
		{
			chip.StrokeThickness = 0;
			chip.SetColor(BackgroundColorProperty, Theme.Accent);
			label.SetColor(Label.TextColorProperty, Theme.OnAccent);
		}
		else
		{
			chip.StrokeThickness = 1;
			chip.SetColor(Border.StrokeProperty, Theme.Outline);
			chip.SetColor(BackgroundColorProperty, Theme.Surface);
			label.SetColor(Label.TextColorProperty, Theme.TextPrimary);
		}
	}
}
