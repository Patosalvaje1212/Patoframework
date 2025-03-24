using Raylib_cs;

using System.Numerics;
using PatoframeWork.Rendering;


using static Raylib_cs.Raylib;
using rlImGui_cs;
using ImGuiNET;
using Newtonsoft.Json;
using PatoframeWork.Physics;


namespace PatoframeWork;
public static class GameController
{

    // Save Location and Filename -- They get initialized after selecting a path
    public static string? SaveLocation = "./";
    public static string? fileSaveName = "";


    // Properties for the save system
    // NOTE: Json saves get too heavy, set Formatting to Formatting.None
    static readonly JsonSerializerSettings settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    // Every object that has to render
    [JsonIgnore]
    private readonly static HashSet<RendererBehaviour> Renderers = [];

    // Dictionary binding every Entity to an Idç
    // TODO: make get entity method
    private static Dictionary<ulong, Entity> Entities = [];



    // Is called once per frame. Used to call Entity.SelfUpdate()
    [JsonIgnore]
    public static Action? Update;

    

    // Frame Counter 
    public static int CurrentFrame { get; private set; } = 0;

    public static void MainThread()
    {
        

        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetConfigFlags(ConfigFlags.AlwaysRunWindow);
        SetConfigFlags(ConfigFlags.MaximizedWindow);
        
        InitWindow(1400, 700, "Hello World");
        
        MaximizeWindow();


        SetExitKey(KeyboardKey.Null);

        Entities = [];

        CurrentFrame = 0;
        
        SetTargetFPS(60);

        Update = new(UpdateGame);

        // ----

        SpriteManager.LoadTextureFolder("./Resources/Images");


        // Only edit mode Setups
        #if DEBUG

            CameraManager.I.freeRoam = true;

            RlImGui.Setup(true);    
        
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;


        #endif
        

        while (!WindowShouldClose())
        {
    

            CurrentFrame ++;

            Update.Invoke();



            // Update Camera
            CameraManager.I.UpdateCamera();


            if(SpriteManager.IsDirty)
            {
                SpriteManager.LoadAllTextures();
            }

            var toRender = Renderers.Where((res) => res.Owner.Active).OrderBy((res) => res.Order).ToList();



            BeginDrawing();

            ClearBackground(Color.Beige);
            

            BeginMode2D(CameraManager.I.cam);


            for (int i = 0; i < toRender.Count; i++)
            {

                if(toRender[i].RenderType == RendererBehaviour.VisualShapeType.Image)
                {            
                    if(SpriteManager.LoadedImages.TryGetValue(toRender[i].ImageID, out ImageData? image))
                    {
                        image.loadedTexture ??= LoadTextureFromImage(image.image);

                        if(image.SpriteRects.TryGetValue(toRender[i].SpriteID, out Rectangle sprite) && image.loadedTexture is Texture2D loadedTexture)
                        {
                            DrawTexturePro(loadedTexture, sprite, new Rectangle(Vector2.Zero, Raymath.Vector2Normalize(sprite.Size) * toRender[i].Size), -toRender[i].Owner.GlobalPosition + Vector2.One * (Raymath.Vector2Normalize(sprite.Size) * toRender[i].Size) / 2, toRender[i].zRot, toRender[i].Color);
                        }
                        else
                        ErrorManager.LogError($"Unexpected missing texture when loading texure ID {toRender[i].ImageID}");
                        
                    } else
                    ErrorManager.LogError($"Could not find Image ID {toRender[i].ImageID}");

                } else if(toRender[i].RenderType == RendererBehaviour.VisualShapeType.Square)
                {
                    DrawRectangle((int)MathF.Round(toRender[i].Owner.GlobalPosition.X) - (int)toRender[i].Size/2, (int)MathF.Round(toRender[i].Owner.GlobalPosition.Y) - (int)toRender[i].Size/2, (int)toRender[i].Size, (int)toRender[i].Size, toRender[i].Color);

                } else if(toRender[i].RenderType == RendererBehaviour.VisualShapeType.Circle)
                {
                    DrawCircle((int)MathF.Round(toRender[i].Owner.GlobalPosition.X), (int)MathF.Round(toRender[i].Owner.GlobalPosition.Y), toRender[i].Size, toRender[i].Color);
                }
            }
            

            
            // Draw all the Debug Windows
            #if DEBUG
                
                InspectorVisual.DrawSelectedPos();
    
                EndMode2D();
          

                RlImGui.Begin();

                InspectorVisual.ImGUIBeh();

                RlImGui.End();
            
            #endif
            
            EndDrawing();
            
            
        }

        #if DEBUG

            RlImGui.Shutdown();

        #endif

        SpriteManager.UnloadAllImages();

        CloseWindow();
    }

    static void UpdateGame()
    {
        PhysicsManager.CallPhysicUpdate();
    }

    #region Scene Data
    // Used by the Serializer to Open Data files, and load its content
    public static void LoadScene()
    {
        
        var data = File.ReadAllText(SaveLocation + fileSaveName);

        // Clean up current entity list
        foreach (var entity in Entities.ToList())
        {
            entity.Value.Delete();
        }


        Dictionary<ulong, Entity>? ConvertedData = JsonConvert.DeserializeObject<Dictionary<ulong, Entity>>(data, settings);

        if(ConvertedData != null) Entities = ConvertedData;
        else
        throw new FileLoadException("Error while loading Data. The target file might not exist, or is an unmatchable Json");
    }

    
    // Used by the Serializer to Save all data to files
    public static void SaveScene()
    {   
        string jsonStringGen = JsonConvert.SerializeObject(Entities, settings);

        File.WriteAllText(SaveLocation + fileSaveName, jsonStringGen);
    }

    #endregion
    
    #region Entity List Helpers

    public static Entity? TryFindEntity(ulong ID)
    {
        return Entities.TryGetValue(ID, out Entity? value) ? value : null;
    }

    public static Entity FindEntity(ulong ID)
    {
        return Entities[ID];
    }

    public static void AddEntity(Entity entity)
    {
        Entities.Add(entity.Id, entity);
    }

    public static void RemoveEntity(ulong ID)
    {
        Entities.Remove(ID);
    }

    public static ulong GetLowestFreeID()
    {
        ulong Lowest = 1;

        List<ulong> List = [.. GameController.Entities.Keys.Order()];
        for (int i = 0; i < List.Count; i++)
        {
            if(Lowest == List[i]) Lowest ++;
        }

        return Lowest;
    }

    public static List<Entity> GetAllEntities()
    {
        return [.. Entities.Values];
    }

    #endregion

    #region Renderer List Helpers
    public static void AddRenderer(RendererBehaviour rendBeh)
    {
        Renderers.Add(rendBeh);
    }

    public static void RemoveRenderer(RendererBehaviour rendBeh)
    {
        Renderers.Remove(rendBeh);
    }

    #endregion
}