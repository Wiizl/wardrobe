using System.Text.Json.Serialization;
using Wardrobe.Models;

namespace Wardrobe.Services;

// Source-generated serializer: safe with trimming in Android Release builds.
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UseStringEnumConverter = true)]
[JsonSerializable(typeof(WardrobeData))]
internal sealed partial class WardrobeJsonContext : JsonSerializerContext
{
}
