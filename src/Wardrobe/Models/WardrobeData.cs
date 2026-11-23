namespace Wardrobe.Models;

/// <summary>Root object of the single JSON data file.</summary>
public sealed class WardrobeData
{
	public int Version { get; set; } = 1;
	public List<WardrobeDefinition> Wardrobes { get; set; } = [];
	public List<ClothingItem> Items { get; set; } = [];
}
