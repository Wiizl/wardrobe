namespace Wardrobe.Models;

public static class ClothingTypes
{
	/// <summary>All types, ordered alphabetically.</summary>
	public static readonly IReadOnlyList<string> All = new[]
	{
		"T-shirt", "Shirt", "Blouse", "Top", "Sweater", "Hoodie", "Cardigan",
		"Jacket", "Coat", "Blazer", "Suit", "Dress", "Skirt",
		"Trousers", "Jeans", "Shorts", "Leggings",
		"Sportswear", "Sleepwear", "Underwear", "Socks", "Swimwear",
		"Shoes", "Scarf", "Hat", "Accessory", "Bedding", "Towel", "Other",
	}.OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase).ToList();
}
