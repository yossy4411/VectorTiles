# VectorTiles.Mvt

This package provides a Reader for Mapbox Vector Tiles (MVT).

## Installation

~~NuGet package: [Okayu.VectorTiles.Mvt](https://www.nuget.org/packages/Okayu.VectorTiles.Mvt/)~~

## Usage

```csharp
using VectorTiles.Mvt;

var tile = MapboxVectorTileReader.Read("path/to/tile.mvt");

foreach (var layer in tile.Layers)
{
    Console.WriteLine(layer.Name);
    foreach (var feature in layer.Features)
    {
        // Console.WriteLine(feature.Geometry);
        foreach (var tag in feature.Tags)
        {
            Console.WriteLine($"{tag.Key}: {tag.Value}");
        }
    }
}
```

## License

[MIT](../LICENSE)