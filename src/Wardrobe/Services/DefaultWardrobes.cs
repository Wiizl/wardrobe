using Wardrobe.Models;

namespace Wardrobe.Services;

/// <summary>
/// Built-in wardrobe layouts. Any layout listed here whose Id is missing from the data file
/// is added on startup, so new wardrobes can be shipped by adding them to this list.
/// </summary>
public static class DefaultWardrobes
{
	public static IEnumerable<WardrobeDefinition> Create()
	{
		yield return MainWardrobe();
		yield return NarrowWardrobe();
	}

	/// <summary>Narrow wardrobe: 5 shelves stacked vertically (1 at the top).</summary>
	static WardrobeDefinition NarrowWardrobe()
	{
		const int shelfCount = 5;
		var w = new WardrobeDefinition
		{
			Id = "narrow",
			Name = "Narrow wardrobe",
			ColumnWeights = [1],
			RowWeights = Enumerable.Repeat(1.0, shelfCount).ToList(),
			AspectRatio = 0.5,
		};

		for (int i = 0; i < shelfCount; i++)
		{
			w.Shelves.Add(new Shelf
			{
				Id = $"narrow-{i + 1}",
				Name = (i + 1).ToString(),
				Row = i,
				Column = 0,
			});
		}

		return w;
	}

	/// <summary>
	/// Layout:
	/// ┌──────────────┬────────────┐
	/// │      1       │     3      │
	/// ├──────────────┼─────┬──────┤
	/// │              │  4  │  5   │
	/// │              ├─────┼──────┤
	/// │      2       │  6  │  7   │
	/// │              ├─────┼──────┤
	/// │              │  8  │  9   │
	/// │              ├─────┼──────┤
	/// │              │ 10  │ 11   │
	/// └──────────────┴─────┴──────┘
	/// </summary>
	static WardrobeDefinition MainWardrobe()
	{
		var w = new WardrobeDefinition
		{
			Id = "main",
			Name = "Main wardrobe",
			ColumnWeights = [2, 1, 1],
			RowWeights = [1, 1, 1, 1, 1],
			AspectRatio = 570.0 / 370.0,
		};

		void Add(int number, int row, int col, int rowSpan = 1, int colSpan = 1) =>
			w.Shelves.Add(new Shelf
			{
				Id = $"main-{number}",
				Name = number.ToString(),
				Row = row,
				Column = col,
				RowSpan = rowSpan,
				ColumnSpan = colSpan,
			});

		Add(1, 0, 0);
		Add(2, 1, 0, rowSpan: 4);
		Add(3, 0, 1, colSpan: 2);
		Add(4, 1, 1);
		Add(5, 1, 2);
		Add(6, 2, 1);
		Add(7, 2, 2);
		Add(8, 3, 1);
		Add(9, 3, 2);
		Add(10, 4, 1);
		Add(11, 4, 2);

		return w;
	}
}
