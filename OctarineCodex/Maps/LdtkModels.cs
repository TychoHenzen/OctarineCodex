using System.Text.Json.Serialization;

namespace OctarineCodex.Maps;

/// <summary>
/// Represents an LDTK project file containing levels and metadata.
/// </summary>
public record LdtkProject
{
    [JsonPropertyName("levels")]
    public LdtkLevel[] Levels { get; init; } = [];

    [JsonPropertyName("defs")]
    public LdtkDefinitions Definitions { get; init; } = new();

    [JsonPropertyName("worldGridWidth")]
    public int WorldGridWidth { get; init; }

    [JsonPropertyName("worldGridHeight")]
    public int WorldGridHeight { get; init; }

    [JsonPropertyName("defaultGridSize")]
    public int DefaultGridSize { get; init; } = 16;
}

/// <summary>
/// Represents definitions section containing tilesets, entities, and layer definitions.
/// </summary>
public record LdtkDefinitions
{
    [JsonPropertyName("tilesets")]
    public LdtkTilesetDefinition[] Tilesets { get; init; } = [];

    [JsonPropertyName("entities")]
    public LdtkEntityDefinition[] Entities { get; init; } = [];

    [JsonPropertyName("layers")]
    public LdtkLayerDefinition[] Layers { get; init; } = [];
}

/// <summary>
/// Represents a single level in an LDTK project.
/// </summary>
public record LdtkLevel
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("uid")]
    public int Uid { get; init; }

    [JsonPropertyName("pxWid")]
    public int PixelWidth { get; init; }

    [JsonPropertyName("pxHei")]
    public int PixelHeight { get; init; }

    [JsonPropertyName("worldX")]
    public int WorldX { get; init; }

    [JsonPropertyName("worldY")]
    public int WorldY { get; init; }

    [JsonPropertyName("layerInstances")]
    public LdtkLayerInstance[] LayerInstances { get; init; } = [];
}

/// <summary>
/// Represents a layer instance within a level.
/// </summary>
public record LdtkLayerInstance
{
    [JsonPropertyName("__identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("__type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("__cWid")]
    public int CellWidth { get; init; }

    [JsonPropertyName("__cHei")]
    public int CellHeight { get; init; }

    [JsonPropertyName("__gridSize")]
    public int GridSize { get; init; }

    [JsonPropertyName("__tilesetDefUid")]
    public int? TilesetDefUid { get; init; }

    [JsonPropertyName("gridTiles")]
    public LdtkTileInstance[] GridTiles { get; init; } = [];

    [JsonPropertyName("entityInstances")]
    public LdtkEntityInstance[] EntityInstances { get; init; } = [];

    [JsonPropertyName("intGridCSV")]
    public int[] IntGridCsv { get; init; } = [];
}

/// <summary>
/// Represents a single tile instance in a tile layer.
/// </summary>
public record LdtkTileInstance
{
    [JsonPropertyName("px")]
    public int[] Px { get; init; } = [];

    [JsonPropertyName("src")]
    public int[] Src { get; init; } = [];

    [JsonPropertyName("t")]
    public int TileId { get; init; }

    [JsonPropertyName("f")]
    public int FlipBits { get; init; }
}

/// <summary>
/// Represents an entity instance in an entity layer.
/// </summary>
public record LdtkEntityInstance
{
    [JsonPropertyName("__identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("__grid")]
    public int[] Grid { get; init; } = [];

    [JsonPropertyName("__pivot")]
    public float[] Pivot { get; init; } = [];

    [JsonPropertyName("px")]
    public int[] Px { get; init; } = [];

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("defUid")]
    public int DefUid { get; init; }

    [JsonPropertyName("fieldInstances")]
    public LdtkFieldInstance[] FieldInstances { get; init; } = [];
}

/// <summary>
/// Represents a field instance for an entity.
/// </summary>
public record LdtkFieldInstance
{
    [JsonPropertyName("__identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("__value")]
    public object? Value { get; init; }

    [JsonPropertyName("__type")]
    public string Type { get; init; } = string.Empty;
}

/// <summary>
/// Represents a tileset definition.
/// </summary>
public record LdtkTilesetDefinition
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("uid")]
    public int Uid { get; init; }

    [JsonPropertyName("relPath")]
    public string RelPath { get; init; } = string.Empty;

    [JsonPropertyName("pxWid")]
    public int PixelWidth { get; init; }

    [JsonPropertyName("pxHei")]
    public int PixelHeight { get; init; }

    [JsonPropertyName("tileGridSize")]
    public int TileGridSize { get; init; }
}

/// <summary>
/// Represents an entity definition.
/// </summary>
public record LdtkEntityDefinition
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("uid")]
    public int Uid { get; init; }

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#FF0000";

    [JsonPropertyName("fieldDefs")]
    public LdtkFieldDefinition[] FieldDefs { get; init; } = [];
}

/// <summary>
/// Represents a field definition for an entity.
/// </summary>
public record LdtkFieldDefinition
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("__type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("defaultOverride")]
    public object? DefaultOverride { get; init; }
}

/// <summary>
/// Represents a layer definition.
/// </summary>
public record LdtkLayerDefinition
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("uid")]
    public int Uid { get; init; }

    [JsonPropertyName("gridSize")]
    public int GridSize { get; init; }
}