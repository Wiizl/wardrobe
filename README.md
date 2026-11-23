# wardrobe
Wardrobe management – .NET 10 MAUI Android app.

## Build / run
```
dotnet build Wardrobe.slnx
dotnet build src/Wardrobe/Wardrobe.csproj -t:Run -f net10.0-android   # deploy to running emulator/device
```

## Structure
- `Models/` – `WardrobeDefinition` (grid-based shelf layout), `Shelf`, `ClothingItem`, `WardrobeData` (JSON root).
- `Services/WardrobeStore.cs` – loads/saves the single JSON file (`<AppData>/wardrobe.json`), images in `<AppData>/images/`.
- `Services/DefaultWardrobes.cs` – built-in layouts; add a new `WardrobeDefinition` here to ship another wardrobe
  (it is merged into the data file on next start and appears in the wardrobe dropdown).
- `Platforms/Android/AndroidImageResizer.cs` – resizes photos to fit 512×512 (aspect ratio kept, EXIF rotation applied).
- `Controls/WardrobeLayoutView.cs` – draws the layout; used for browsing and for choosing a shelf.
- `Pages/` – main (wardrobe dropdown + layout), shelf (item list), item editor (add / edit / move / delete).
