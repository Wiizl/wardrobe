using System.Text.Json;
using Wardrobe.Models;

namespace Wardrobe.Services;

/// <summary>
/// Owns the single JSON data file (wardrobes + clothing items) and the local image folder.
/// </summary>
public sealed class WardrobeStore(IImageResizer resizer)
{
	public const int ImageSize = 512;
	const string DataFileName = "wardrobe.json";
	const string SelectedWardrobeKey = "selected_wardrobe";

	readonly SemaphoreSlim _lock = new(1, 1);
	Task? _initTask;

	public static string ImagesDirectory => System.IO.Path.Combine(FileSystem.AppDataDirectory, "images");
	static string PendingDirectory => System.IO.Path.Combine(FileSystem.CacheDirectory, "pending-images");
	public static string DataFilePath => System.IO.Path.Combine(FileSystem.AppDataDirectory, DataFileName);

	public WardrobeData Data { get; private set; } = new();

	public IReadOnlyList<WardrobeDefinition> Wardrobes => Data.Wardrobes;

	public string SelectedWardrobeId
	{
		get
		{
			var id = Preferences.Default.Get(SelectedWardrobeKey, "");
			return Data.Wardrobes.Any(w => w.Id == id) ? id : Data.Wardrobes.FirstOrDefault()?.Id ?? "";
		}
		set => Preferences.Default.Set(SelectedWardrobeKey, value);
	}

	public Task InitializeAsync() => _initTask ??= LoadAsync();

	public WardrobeDefinition? GetWardrobe(string? id) => Data.Wardrobes.FirstOrDefault(w => w.Id == id);

	public List<ClothingItem> GetItems(string wardrobeId, string shelfId) =>
		Data.Items
			.Where(i => i.WardrobeId == wardrobeId && i.ShelfId == shelfId)
			.OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
			.ToList();

	public Dictionary<string, int> GetShelfCounts(string wardrobeId) =>
		Data.Items
			.Where(i => i.WardrobeId == wardrobeId)
			.GroupBy(i => i.ShelfId)
			.ToDictionary(g => g.Key, g => g.Count());

	/// <summary>
	/// Resizes the picked image to fit 512×512 and stores it in a temporary location.
	/// It only becomes permanent when the item is saved.
	/// </summary>
	public async Task<string> ImportPendingImageAsync(Stream source)
	{
		Directory.CreateDirectory(PendingDirectory);
		var target = System.IO.Path.Combine(PendingDirectory, Guid.NewGuid().ToString("N"));
		return await resizer.ResizeAsync(source, target, ImageSize);
	}

	/// <summary>Adds or updates an item. <paramref name="pendingImagePath"/> replaces its image if set.</summary>
	public async Task SaveItemAsync(ClothingItem item, string? pendingImagePath)
	{
		await _lock.WaitAsync();
		try
		{
			string? oldImage = null;
			if (pendingImagePath is not null)
			{
				Directory.CreateDirectory(ImagesDirectory);
				var fileName = Guid.NewGuid().ToString("N") + System.IO.Path.GetExtension(pendingImagePath);
				File.Move(pendingImagePath, System.IO.Path.Combine(ImagesDirectory, fileName));
				oldImage = item.ImageFileName;
				item.ImageFileName = fileName;
			}

			var index = Data.Items.FindIndex(i => i.Id == item.Id);
			if (index >= 0)
				Data.Items[index] = item;
			else
				Data.Items.Add(item);

			await PersistAsync();
			DeleteImageFile(oldImage);
		}
		finally
		{
			_lock.Release();
		}
	}

	public async Task DeleteItemAsync(string itemId)
	{
		await _lock.WaitAsync();
		try
		{
			var item = Data.Items.FirstOrDefault(i => i.Id == itemId);
			if (item is null)
				return;
			Data.Items.Remove(item);
			await PersistAsync();
			DeleteImageFile(item.ImageFileName);
		}
		finally
		{
			_lock.Release();
		}
	}

	async Task LoadAsync()
	{
		await _lock.WaitAsync();
		try
		{
			if (File.Exists(DataFilePath))
			{
				await using var stream = File.OpenRead(DataFilePath);
				Data = await JsonSerializer.DeserializeAsync(stream, WardrobeJsonContext.Default.WardrobeData) ?? new WardrobeData();
			}

			// Add any built-in wardrobe that is not in the file yet.
			bool changed = false;
			foreach (var def in DefaultWardrobes.Create())
			{
				if (Data.Wardrobes.All(w => w.Id != def.Id))
				{
					Data.Wardrobes.Add(def);
					changed = true;
				}
			}

			if (changed || !File.Exists(DataFilePath))
				await PersistAsync();
		}
		finally
		{
			_lock.Release();
		}
	}

	async Task PersistAsync()
	{
		Directory.CreateDirectory(FileSystem.AppDataDirectory);
		var tmp = DataFilePath + ".tmp";
		await using (var stream = File.Create(tmp))
		{
			await JsonSerializer.SerializeAsync(stream, Data, WardrobeJsonContext.Default.WardrobeData);
		}
		File.Move(tmp, DataFilePath, overwrite: true);
	}

	static void DeleteImageFile(string? fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return;
		try
		{
			File.Delete(System.IO.Path.Combine(ImagesDirectory, fileName));
		}
		catch (IOException)
		{
			// Not critical – an orphaned image only wastes space.
		}
	}
}
