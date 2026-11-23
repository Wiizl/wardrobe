namespace Wardrobe.Services;

public interface IImageResizer
{
	/// <summary>
	/// Decodes <paramref name="input"/>, scales it to fit inside a
	/// <paramref name="maxSize"/>×<paramref name="maxSize"/> box keeping the aspect ratio,
	/// and writes it next to <paramref name="destinationPathWithoutExtension"/>.
	/// Returns the full path of the written file (extension chosen by the resizer).
	/// </summary>
	Task<string> ResizeAsync(Stream input, string destinationPathWithoutExtension, int maxSize);
}
