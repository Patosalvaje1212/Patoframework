using PF.Physics;

namespace PF.Tilemap;


public class TilesetRenderSystem : ActorSystem
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    Dictionary<Actor, Tiledmap> tilemapMapper;
    Dictionary<Actor, Transform> transformMapper;

    public TilesetRenderSystem(World world) : base(world)
    {
        AddRequiredType(typeof(Tiledmap));
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void Init()
    {
        transformMapper = world.GetMapper<Transform>();
        tilemapMapper = world.GetMapper<Tiledmap>();
    }

    public override void Draw(nint renderer)
    {
        foreach (var actorID in mActors)
        {
            Tiledmap tilemap = tilemapMapper[actorID];

            transformMapper.TryGetValue(actorID, out Transform? transform);

            unsafe
            {
                foreach (int[,] layer in tilemap.tileLayers)
                {

                    for (int j = 0; j < tilemap.Height; j++)
                    {
                        for (int i = 0; i < tilemap.Width; i++)
                        {
                            if (layer[i, j] == 0) continue;


                            try
                            {
                                TilesetData source = tilemap.tilesets.First((res) => res.IsTileIdInTileset(layer[i, j]));

                                int tileId = layer[i, j] - 1;

                                /* SDL.FRect sourceRect = new()
                                {
                                    X = source.TileWidth * ((tileId) % source.Width),
                                    Y = source.TileHeight * ((tileId) / source.Width),

                                    W = source.TileWidth,
                                    H = source.TileHeight
                                };

                                SDL.FRect destRect = new()
                                {
                                    X = (tilemap.TileWidth * i + (transform != null ? transform.Position.X : 0)) * tilemap.Scale,
                                    Y = (tilemap.TileHeight * j + (transform != null ? transform.Position.Y : 0)) * tilemap.Scale,

                                    W = tilemap.TileWidth * tilemap.Scale,
                                    H = tilemap.TileHeight * tilemap.Scale
                                };

                                SDL.RenderTexture(
                                    renderer,
                                    source.SourceTexture,
                                    sourceRect,
                                    destRect
                                ); */

                                //Logger.Log("Rect : " + sourceRect.X + ", " + sourceRect.Y);

                            }
                            catch (InvalidOperationException e)
                            {
                                throw new Exception($"Could not find tile with ID: {layer[i, j]} in any loaded tileset.", e);
                            }


                        }
                    }
                }


            }

        }
    }


    public override void Update(double deltaTime) { }
}