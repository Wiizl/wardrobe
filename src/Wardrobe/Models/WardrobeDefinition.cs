namespace Wardrobe.Models;

/// <summary>
/// A wardrobe and its shelf layout. The layout is a weighted grid: each shelf occupies
/// one or more cells (Row/Column + spans), so any rectangular layout can be described in JSON.
/// </summary>
public sealed class WardrobeDefinition
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");
	public string Name { get; set; } = "Wardrobe";

	/// <summary>Relative widths of the grid columns, e.g. [2, 1, 1].</summary>
	public List<double> ColumnWeights { get; set; } = [1];

	/// <summary>Relative heights of the grid rows.</summary>
	public List<double> RowWeights { get; set; } = [1];

	/// <summary>Width / height of the drawn wardrobe.</summary>
	public double AspectRatio { get; set; } = 1.5;

	public List<Shelf> Shelves { get; set; } = [];

	public Shelf? FindShelf(string? shelfId) => Shelves.FirstOrDefault(s => s.Id == shelfId);
}

public sealed class Shelf
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");
	public string Name { get; set; } = "";
	public int Row { get; set; }
	public int Column { get; set; }
	public int RowSpan { get; set; } = 1;
	public int ColumnSpan { get; set; } = 1;
}
