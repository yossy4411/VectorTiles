# VectorTiles.Styles

This project contains the styles for the vector tiles.

## Supported Styles

- Mapbox Style (JSON)

## Usage

```csharp
using VectorTiles.Styles;

var text = File.ReadAllText("path/to/style.json");
var style = MapboxStyle.FromJson(text);
```

## License

This project is licensed under the MIT License - see the [LICENSE](/LICENSE) file for details.
