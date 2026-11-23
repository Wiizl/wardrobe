using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace Wardrobe.Controls;

public static class ViewHelpers
{
	/// <summary>Adds a tap handler with a short "pressed" fade.</summary>
	public static T OnTapped<T>(this T view, Action action) where T : View
	{
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
		{
			_ = PressFeedbackAsync(view);
			action();
		};
		view.GestureRecognizers.Add(tap);
		return view;
	}

	static async Task PressFeedbackAsync(View view)
	{
		await view.FadeToAsync(0.55, 60);
		await view.FadeToAsync(1, 140);
	}

	public static Style? Style(string key) =>
		Application.Current?.Resources.TryGetValue(key, out var value) == true ? value as Style : null;
}

/// <summary>Simple stroked line icons drawn in a 24×24 box.</summary>
public static class Icons
{
	public const string Back = "M15,5 L8,12 L15,19";
	public const string ChevronDown = "M6,9 L12,15 L18,9";
	public const string ChevronRight = "M9,5 L16,12 L9,19";
	public const string Plus = "M12,5 L12,19 M5,12 L19,12";
	public const string Check = "M5,12.5 L10,17.5 L19,7";

	static readonly PathGeometryConverter Converter = new();

	public static Path Create(string data, string themeColor, double size = 24, double thickness = 2.2)
	{
		var path = CreateRaw(data, size, thickness);
		path.SetColor(Shape.StrokeProperty, themeColor);
		return path;
	}

	public static Path Create(string data, Color color, double size = 24, double thickness = 2.2)
	{
		var path = CreateRaw(data, size, thickness);
		path.Stroke = color;
		return path;
	}

	static Path CreateRaw(string data, double size, double thickness)
	{
		var path = new Path
		{
			Data = (Geometry)Converter.ConvertFromInvariantString(data)!,
			StrokeThickness = thickness,
			StrokeLineCap = PenLineCap.Round,
			StrokeLineJoin = PenLineJoin.Round,
			WidthRequest = 24,
			HeightRequest = 24,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			InputTransparent = true,
		};
		if (Math.Abs(size - 24) > 0.1)
			path.Scale = size / 24; // geometry is authored for 24×24 (layout box stays 24)
		return path;
	}
}
