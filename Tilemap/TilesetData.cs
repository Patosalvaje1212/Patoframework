


using System.Text.Json.Nodes;
using PF.Visual;
using Raylib_cs;

namespace PF.Tilemap;

public class TilesetData
{
    public Texture2D SourceTexture { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }

    public int FirstTileID { get; private set; }
    public string Name { get; private set; }

    public TilesetData(JsonNode tilesetJson, string originPath)
    {
        Width = (int)tilesetJson!["columns"]!;
        Height = (int)tilesetJson!["tilecount"]! / Width;

        TileWidth = (int)tilesetJson!["tilewidth"]!;
        TileHeight = (int)tilesetJson!["tileheight"]!;

        FirstTileID = (int)tilesetJson!["firstgid"]!;
        Name = (string)tilesetJson!["name"]!;

        SourceTexture = ResourceManager.Load.GetTexture(Name);
    }

    public bool IsTileIdInTileset(int tileId)
    {
        return tileId >= FirstTileID && tileId < FirstTileID + Width * Height;
    }
}