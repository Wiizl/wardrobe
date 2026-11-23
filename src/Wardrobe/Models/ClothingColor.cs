namespace Wardrobe.Models;

/// <summary>Predefined clothing colors. Stored by name in the JSON file.</summary>
public enum ClothingColor
{
	Beige,
	Black,
	Blue,
	Brown,
	Burgundy,
	Gold,
	Gray,
	Green,
	Khaki,
	LightBlue,
	Multicolor,
	Navy,
	Olive,
	Orange,
	Pink,
	Purple,
	Red,
	Silver,
	Turquoise,
	White,
	Yellow,
}

public static class ClothingColors
{
	/// <summary>All colors, ordered alphabetically by display name.</summary>
	public static readonly IReadOnlyList<ClothingColor> All =
		Enum.GetValues<ClothingColor>().OrderBy(DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();

	public static string DisplayName(ClothingColor color) => color switch
	{
		ClothingColor.LightBlue => "Light blue",
		_ => color.ToString(),
	};

	/// <summary>Swatch color for the UI.</summary>
	public static Color Swatch(ClothingColor color) => Color.FromArgb(color switch
	{
		ClothingColor.Beige => "#E8D8B8",
		ClothingColor.Black => "#000000",
		ClothingColor.Blue => "#1E63D6",
		ClothingColor.Brown => "#6D4C41",
		ClothingColor.Burgundy => "#800020",
		ClothingColor.Gold => "#D4AF37",
		ClothingColor.Gray => "#8C8C8C",
		ClothingColor.Green => "#2E7D32",
		ClothingColor.Khaki => "#C3B091",
		ClothingColor.LightBlue => "#90CAF9",
		ClothingColor.Multicolor => "#B0B0B0",
		ClothingColor.Navy => "#1A237E",
		ClothingColor.Olive => "#6B7A2A",
		ClothingColor.Orange => "#F57C00",
		ClothingColor.Pink => "#F48FB1",
		ClothingColor.Purple => "#7B1FA2",
		ClothingColor.Red => "#D32F2F",
		ClothingColor.Silver => "#C0C0C0",
		ClothingColor.Turquoise => "#26C6DA",
		ClothingColor.White => "#FFFFFF",
		ClothingColor.Yellow => "#FDD835",
		_ => "#B0B0B0",
	});
}
