using System.Numerics;
using System.Text.Json.Nodes;
using PF.Physics;
using PF.Visual;

namespace PF.Tilemap;

public class Tiledmap
{
    public int TileWidth, TileHeight;
    public int Width, Height;

    public List<Sprite> tiles = [];

    public List<int[,]> tileLayers;
    public Dictionary<string, List<JsonNode>> objectLayers;

    public List<TilesetData> tilesets;

    public List<Actor> createdActors = [];


    public bool IsReady { get; private set; } = false;

    public float Scale = 1;

#pragma warning disable CS8604 // Possible null reference argument.

    public Tiledmap(string path)
    {
        //using FileStream stream = new(path, FileMode.Open);
        string textData = File.ReadAllText(path);

        JsonNode data = JsonNode.Parse(textData) ?? throw new NullReferenceException("ERROR");

        TileWidth = (int)data!["tilewidth"]!;
        TileHeight = (int)data!["tileheight"]!;


        Width = (int)data!["width"]!;
        Height = (int)data!["height"]!;

        tileLayers = [];
        objectLayers = [];

        JsonArray layers = data!["layers"]!.AsArray();


        for (int l = 0; l < layers.Count; l++)
        {
            if ((string)layers[l]!["type"]! == "tilelayer")
            {
                JsonArray cLayer = layers[l]!["data"]!.AsArray();

                tileLayers.Add(new int[Width, Height]);

                for (int j = 0; j < Height; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        tileLayers[^1][i, j] = (int)cLayer[(j * Width) + i];
                    }
                }
            }
            else
            if ((string)layers[l]!["type"]! == "objectgroup")
            {
                JsonArray cLayer = layers[l]!["objects"]!.AsArray();

                List<JsonNode> resList = [];

                for (int i = 0; i < cLayer.Count; i++)
                {
                    resList.Add(cLayer[i]);
                }

                objectLayers.Add((string)layers[l]!["name"]!, resList);
            }


        }

        tilesets = [];
        JsonArray tilesetArray = data!["tilesets"]!.AsArray();

        for (int t = 0; t < tilesetArray.Count; t++)
        {
            tilesets.Add(new TilesetData(tilesetArray[t], path));
        }
    }


    public List<Actor> CreateCollisions(World world, string layerName)
    {
        return CreateActorsByLayer(world, layerName, (actor, jsonActor) =>
        {
            float x = (float)jsonActor!["x"]!;
            float y = (float)jsonActor!["y"]!;

            Transform t = new(new Vector2(x, y) * Scale);
            world.AddComponent(actor, t);
            world.AddComponent(actor, new Rigidbody(1, Rigidbody.BodyType.Static));

            Shape? s = null;

            if (jsonActor.AsObject().ContainsKey("ellipse"))
            {
                bool l = (bool)jsonActor!["ellipse"]!;

                s = new(Shape.ShapeType.Circle, ((float)jsonActor!["width"]! + (float)jsonActor!["height"]!) / 4f * Scale);
                world.AddComponent(actor, s);
            }
            else
            {
                s = new(Shape.ShapeType.Rectangle, (float)jsonActor!["width"]! * Scale, (float)jsonActor!["height"]! * Scale);
                world.AddComponent(actor, s);
            }

            if (jsonActor.AsObject().ContainsKey("point"))
                throw new InvalidOperationException("Could not create a collider from the type 'point'. Please use a 'rectangle' or 'ellipse'.");

            t.Position += s.Size / 2f;
        });
    }
#pragma warning restore CS8604 // Possible null reference argument.

    public List<Actor> CreateActorsByLayer(World world, string layerName, Action<Actor, JsonNode> action)
    {
        var layer = objectLayers[layerName];

        List<Actor> res = [];

        foreach (JsonNode jsonActor in layer)
        {
            Actor actor = world.CreateActor();

            action.Invoke(actor, jsonActor);

            res.Add(actor);
        }

        return res;
    }

    public List<Actor> CreateActorsByName(World world, string objectName, Action<Actor, JsonNode> action)
    {
        List<JsonNode> matching = [.. objectLayers.Values.SelectMany(
            (res) =>
                res.Where(
                    (rem) =>
                        rem.AsObject().ContainsKey("point")
                        && rem.AsObject().TryGetPropertyValue("name", out JsonNode? name)
                        && name != null
                        && name.GetValue<string>() == objectName
                )
        )];

        List<Actor> res = [];

        foreach (JsonNode jsonActor in matching)
        {
            Actor actor = world.CreateActor();

            action.Invoke(actor, jsonActor);

            res.Add(actor);
        }

        return res;
    }

}