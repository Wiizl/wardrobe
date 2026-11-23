namespace Wardrobe.Controls;

/// <summary>Dropdown-looking field; raises <see cref="Tapped"/> so the page can show a <see cref="BottomSheet"/>.</summary>
public sealed class SelectField : ContentView
{
	readonly Label _label = new() { FontSize = 16, VerticalOptions = LayoutOptions.Center };
	string _text = "";
	string _placeholder = "";

	public event EventHandler? Tapped;

	public SelectField()
	{
		var grid = new Grid
		{
			ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) },
			MinimumHeightRequest = 52,
		};
		grid.Add(_label, 0);
		grid.Add(Icons.Create(Icons.ChevronDown, Theme.TextSecondary, size: 20), 1);

		var border = new Border { Style = ViewHelpers.Style("FieldBorder"), Content = grid };
		border.OnTapped(() => Tapped?.Invoke(this, EventArgs.Empty));
		Content = border;
		Update();
	}

	public string Text
	{
		get => _text;
		set { _text = value ?? ""; Update(); }
	}

	public string Placeholder
	{
		get => _placeholder;
		set { _placeholder = value ?? ""; Update(); }
	}

	void Update()
	{
		bool empty = string.IsNullOrEmpty(_text);
		_label.Text = empty ? _placeholder : _text;
		_label.SetColor(Label.TextColorProperty, empty ? Theme.TextSecondary : Theme.TextPrimary);
	}
}
