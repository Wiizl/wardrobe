using Android.Graphics;
using Android.Media;
using Stream = System.IO.Stream;

namespace Wardrobe.Services;

public sealed class AndroidImageResizer : IImageResizer
{
	public async Task<string> ResizeAsync(Stream input, string destinationPathWithoutExtension, int maxSize)
	{
		byte[] bytes;
		using (var ms = new MemoryStream())
		{
			await input.CopyToAsync(ms);
			bytes = ms.ToArray();
		}

		return await Task.Run(() => Resize(bytes, destinationPathWithoutExtension, maxSize));
	}

	static string Resize(byte[] bytes, string destinationPathWithoutExtension, int maxSize)
	{
		// 1. Read dimensions only, then pick a power-of-two sample size so we never
		//    decode a full 12+ MP photo into memory.
		var bounds = new BitmapFactory.Options { InJustDecodeBounds = true };
		BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, bounds);
		if (bounds.OutWidth <= 0 || bounds.OutHeight <= 0)
			throw new InvalidOperationException("The selected file is not a supported image.");

		int longest = Math.Max(bounds.OutWidth, bounds.OutHeight);
		int sample = 1;
		while (longest / (sample * 2) >= maxSize)
			sample *= 2;

		using var decoded = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, new BitmapFactory.Options { InSampleSize = sample })
			?? throw new InvalidOperationException("The selected image could not be decoded.");

		// 2. Scale to fit the maxSize×maxSize box (aspect ratio kept) and apply EXIF rotation.
		//    The box is square, so rotation does not change the scale factor.
		float scale = Math.Min((float)maxSize / decoded.Width, (float)maxSize / decoded.Height);
		var matrix = new Matrix();
		matrix.PostScale(scale, scale);
		int rotation = ReadExifRotation(bytes);
		if (rotation != 0)
			matrix.PostRotate(rotation);

		var result = Bitmap.CreateBitmap(decoded, 0, 0, decoded.Width, decoded.Height, matrix, true)
			?? throw new InvalidOperationException("The image could not be resized.");

		try
		{
			// 3. Keep transparency as PNG, everything else as JPEG.
			bool png = result.HasAlpha;
			string path = destinationPathWithoutExtension + (png ? ".png" : ".jpg");
			using (var file = File.Create(path))
			{
				result.Compress(png ? Bitmap.CompressFormat.Png! : Bitmap.CompressFormat.Jpeg!, 90, file);
			}
			return path;
		}
		finally
		{
			if (!ReferenceEquals(result, decoded))
				result.Dispose();
		}
	}

	static int ReadExifRotation(byte[] bytes)
	{
		try
		{
			using var ms = new MemoryStream(bytes);
			var exif = new ExifInterface(ms);
			return exif.GetAttributeInt(ExifInterface.TagOrientation, 1) switch
			{
				3 => 180,
				6 => 90,
				8 => 270,
				_ => 0,
			};
		}
		catch
		{
			return 0; // No / unreadable EXIF (e.g. PNG) – leave as is.
		}
	}
}
