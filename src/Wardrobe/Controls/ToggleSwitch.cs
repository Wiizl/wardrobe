using Microsoft.Maui.Controls.Shapes;

namespace Wardrobe.Controls;

/// <summary>App-drawn on/off switch.</summary>
public sealed class ToggleSwitch : ContentView
{
	const double TrackWidth = 52;
	const double TrackHeight = 32;
	const double KnobSize = 24;
	const double KnobInset = 4;
	const double Travel = TrackWidth - KnobSize - 2 * KnobInset;

	readonly Border _track;
	readonly Border _knob;
	bool _isToggled;

	public event EventHandler? Toggled;

	public ToggleSwitch()
	{
		_knob = new Border
		{
			WidthRequest = KnobSize,
			HeightRequest = KnobSize,
			StrokeThickness = 0,
			StrokeShape = new Ellipse(),
			BackgroundColor = Colors.White,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
			Margin = new Thickness(KnobInset, 0),
			InputTransparent = true,
			Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.25f, Radius = 4, Offset = new Point(0, 1) },
		};

		_track = new Border
		{
			WidthRequest = TrackWidth,
			HeightRequest = TrackHeight,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = TrackHeight / 2 },
			VerticalOptions = LayoutOptions.Center,
			Content = _knob,
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => Toggle();
		_track.GestureRecognizers.Add(tap);

		Content = _track;
		Update(animate: false);
	}

	public bool IsToggled
	{
		get => _isToggled;
		set
		{
			if (_isToggled == value)
				return;
			_isToggled = value;
			Update(animate: Handler is not null);
		}
	}

	public void Toggle()
	{
		IsToggled = !IsToggled;
		Toggled?.Invoke(this, EventArgs.Empty);
	}

	void Update(bool animate)
	{
		_track.SetColor(BackgroundColorProperty, _isToggled ? Theme.Accent : Theme.Outline);
		SemanticProperties.SetDescription(this, _isToggled ? "On" : "Off");

		var x = _isToggled ? Travel : 0;
		if (animate)
			_ = _knob.TranslateToAsync(x, 0, 150, Easing.CubicOut);
		else
			_knob.TranslationX = x;
	}
}
