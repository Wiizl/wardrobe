namespace Wardrobe.Controls;

/// <summary>Access to the palette in Resources/Styles/Colors.xaml from C# controls.</summary>
public static class Theme
{
	public static Color Get(string name, bool dark)
	{
		var key = name + (dark ? "Dark" : "Light");
		return Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color c
			? c
			: Colors.Magenta; // makes a typo in a color name obvious
	}

	/// <summary>Binds <paramref name="property"/> to the light/dark variant of palette color <paramref name="name"/>.</summary>
	public static T SetColor<T>(this T element, BindableProperty property, string name) where T : BindableObject
	{
		element.SetAppThemeColor(property, Get(name, false), Get(name, true));
		return element;
	}

	public const string Background = "Background";
	public const string Surface = "Surface";
	public const string SurfaceVariant = "SurfaceVariant";
	public const string Outline = "Outline";
	public const string ShelfFrame = "ShelfFrame";
	public const string Shelf = "Shelf";
	public const string TextPrimary = "TextPrimary";
	public const string TextSecondary = "TextSecondary";
	public const string Accent = "Accent";
	public const string AccentSoft = "AccentSoft";
	public const string OnAccent = "OnAccent";
	public const string Danger = "Danger";
	public const string Scrim = "Scrim";

	public const string FontRegular = "OpenSansRegular";
	public const string FontSemibold = "OpenSansSemibold";
}
