using System.Text.Json.Serialization;
using Wardrobe.Services;

namespace Wardrobe.Models;

public sealed class ClothingItem
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");
	public string Name { get; set; } = "";
	public ClothingColor? Color { get; set; }
	public string Type { get; set; } = "";

	/// <summary>File name inside the app's images folder (not a full path).</summary>
	public string? ImageFileName { get; set; }

	/// <summary>Buried somewhere at the bottom of the hanger/pile.</summary>
	public bool IsAtBottom { get; set; }

	public string WardrobeId { get; set; } = "";
	public string ShelfId { get; set; } = "";

	[JsonIgnore]
	public string? ImagePath => string.IsNullOrEmpty(ImageFileName)
		? null
		: Path.Combine(WardrobeStore.ImagesDirectory, ImageFileName);

	[JsonIgnore]
	public string ColorName => Color is { } c ? ClothingColors.DisplayName(c) : "";

	[JsonIgnore]
	public Microsoft.Maui.Graphics.Color ColorSwatch => Color is { } c ? ClothingColors.Swatch(c) : Colors.Transparent;

	[JsonIgnore]
	public bool HasType => !string.IsNullOrWhiteSpace(Type);

	[JsonIgnore]
	public bool HasColor => Color is not null;

	[JsonIgnore]
	public string Details => string.Join(" · ", new[] { Type, ColorName }.Where(s => !string.IsNullOrWhiteSpace(s)));

	public ClothingItem Clone() => (ClothingItem)MemberwiseClone();
}
