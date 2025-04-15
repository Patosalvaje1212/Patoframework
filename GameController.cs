using System.Numerics;

using Newtonsoft.Json;


using Raylib_cs;
using static Raylib_cs.Raylib;

using rlImGui_cs;
using ImGuiNET;

using PatoFramework.Inspector;
using PatoFramework.Rendering;
using System.Reflection;
namespace PatoFramework;

/// <summary>
/// Base class for the game Loop, it holds all the Enitites.
/// </summary>

public static class GameController
{

    // Save Location and Filename -- They get initialized after selecting a path

    public static string ProjectLoc { get; } = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "");
    public static string SaveLocation = ProjectLoc;

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

    public static LightBehaviour[] lights = [];



    // Is called once per frame. Used to call Entity.SelfUpdate()
    [JsonIgnore]
    public static Action? Update;

    

    // Frame Counter 
    public static int CurrentFrame { get; private set; } = 0;

    public static float PixelRatio = 4;


    private static readonly int WindowW = 640;
    
    private static readonly int WindowH = 360;

    public static readonly Vector2 WindowPixelSize = new(WindowW, WindowH);


    public static void MainThread()
    {
        

        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetConfigFlags(ConfigFlags.AlwaysRunWindow);
        SetConfigFlags(ConfigFlags.MaximizedWindow);
        
        InitWindow(1400, 800, "Hello World");
        
        MaximizeWindow();


        SetExitKey(KeyboardKey.Null);

        Entities = [];

        CurrentFrame = 0;
        
        SetTargetFPS(60);

        Update = new(UpdateGame);


        SetupFolders();

        // Init Default Texture folder
        //SpriteManager.LoadTextureFolder("/Resources/Images");






        // Only edit mode Setups
        #if DEBUG

            CameraManager.I.freeRoam = true;

            RlImGui.Setup(true);    
        
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;


        #endif

        // Shader Setup:
        #region Default Shader Setup

        bool firstLoad = true;

       
        

        Shader DefaultShader = LoadShader(null, "Rendering/Shaders/DefaultNormal.fs");


        int normalMapLoc = GetShaderLocation(DefaultShader, "texture1");

        int lightPosesLoc = GetShaderLocation(DefaultShader, "LightPos");
        int lNumberLoc = GetShaderLocation(DefaultShader, "LightCount");


        int rLoc = GetShaderLocation(DefaultShader, "Resolution");
        int lrLoc = GetShaderLocation(DefaultShader, "lightResolution");

        int camOffLoc = GetShaderLocation(DefaultShader, "cameraOffset");
        int camZoomLoc = GetShaderLocation(DefaultShader, "cameraZoom");

        int fallOffLoc = GetShaderLocation(DefaultShader, "Falloff");
        int lightColorLoc = GetShaderLocation(DefaultShader, "LightColor");
        int ambientColorLoc = GetShaderLocation(DefaultShader, "AmbientColor");

        Texture2D normalMap = new();

        #endregion

        

        RenderTexture2D texture2D = LoadRenderTexture(WindowW, WindowH);

        //SetTextureFilter(texture2D.Texture, TextureFilter.Trilinear);

        #region GameLoop

        while (!WindowShouldClose())
        {
            CurrentFrame ++;

            Update.Invoke();


            // Update Camera
            CameraManager.I.UpdateCamera();


            if(SpriteManager.IsDirty || firstLoad)
            {
                SpriteManager.LoadAllTextures();
            }


            // Load Shader Variables
            if(LightsManager.IsDirty() || firstLoad)
            {
                
                var LightPoses = LightsManager.GetNearestLights(5);

                var LightColors = LightsManager.GetNearestLightsColors();

                SetShaderValue(DefaultShader, lNumberLoc, LightPoses.Length, ShaderUniformDataType.Int);

                SetShaderValueV(DefaultShader, lightPosesLoc, LightPoses, ShaderUniformDataType.Vec3, LightPoses.Length);
                
                SetShaderValue(DefaultShader, rLoc, WindowPixelSize, ShaderUniformDataType.Vec2);

                SetShaderValue(DefaultShader, fallOffLoc, LightsManager.LightFallOff, ShaderUniformDataType.Vec3);
                SetShaderValueV(DefaultShader, lightColorLoc, LightColors, ShaderUniformDataType.Vec4, LightPoses.Length);

                
                Vector4 ambientLight = ColorNormalize(LightsManager.AmbientLight);

                SetShaderValue(DefaultShader, ambientColorLoc, ambientLight, ShaderUniformDataType.Vec4);


                firstLoad = false;
            }
        
            

            var toRender = Renderers.Where((res) => res.Owner.Active).OrderBy((res) => res.Order).ToList();

            BeginTextureMode(texture2D);
            
            BeginMode2D(CameraManager.I.Cam);
            
            ClearBackground(Color.Beige);
            
            
            BeginShaderMode(DefaultShader);

                
                #if DEBUG

                    InspectorVisual.DrawSelectedPos();
                
                #endif
        

                for (int i = 0; i < toRender.Count; i++)
                {
                    var smoothPos = toRender[i].Owner.GlobalPosition;
                    smoothPos.X = (int)MathF.Round(MathF.Round(smoothPos.X * PixelRatio / 10 ) * 10 / PixelRatio);
                    smoothPos.Y = (int)MathF.Round(MathF.Round(-smoothPos.Y * PixelRatio / 10 ) * 10 / PixelRatio);

                    if(toRender[i].RenderType == RendererBehaviour.VisualShapeType.Image)
                    {            
                        if(SpriteManager.LoadedImages.TryGetValue(toRender[i].ImageID, out ImageData? image))
                        {
                            normalMap = image.loadedNormal;
                            Console.WriteLine(image.hasNormal);

                            SetShaderValueTexture(DefaultShader, normalMapLoc, normalMap);

                            if(image.SpriteRects.TryGetValue(toRender[i].SpriteID, out Rectangle sprite) && image.loadedTexture is Texture2D loadedTexture)
                            {
                                Vector2 spriteSize = Raymath.Vector2Normalize(sprite.Size) * toRender[i].Size * 10;

                                DrawTexturePro(loadedTexture, new Rectangle(sprite.Position, new Vector2(sprite.Width, -sprite.Height)), new Rectangle(smoothPos, spriteSize),
                                (toRender[i].Owner.Parent != 0 ? smoothPos : Vector2.Zero) + spriteSize /  2,
                                toRender[i].zRot, toRender[i].Color);
                            }
                            else
                            LogManager.LogError($"Unexpected missing texture when loading texure ID {toRender[i].ImageID}");
                            
                        } else
                        LogManager.LogError($"Could not find Image ID {toRender[i].ImageID}");

                    } else if(toRender[i].RenderType == RendererBehaviour.VisualShapeType.Square)
                    {
                        DrawRectangle((int)MathF.Round(smoothPos.X) - (int)toRender[i].Size/2, (int)MathF.Round(smoothPos.Y) - (int)toRender[i].Size/2, (int)toRender[i].Size, (int)toRender[i].Size, toRender[i].Color);

                    } else if(toRender[i].RenderType == RendererBehaviour.VisualShapeType.Circle)
                    {
                     
                        DrawCircle((int)MathF.Round(smoothPos.X), (int)MathF.Round(smoothPos.Y), toRender[i].Size, toRender[i].Color);
                    }

                }
                
            EndShaderMode();
            EndMode2D();
            EndTextureMode();


            BeginDrawing();

            //BeginShaderMode(PixelateShader);
                ClearBackground(Color.Black);

                var size = new Vector2(texture2D.Texture.Width, texture2D.Texture.Height);

                var screenSize = Raymath.Vector2Normalize(size) * Raymath.Vector2Length(new Vector2(GetScreenWidth(), GetScreenHeight()));

                DrawTexturePro(texture2D.Texture, new Rectangle(0, 0, size), new Rectangle((new Vector2(GetScreenWidth(), GetScreenHeight()) - screenSize ) / 2, screenSize), Vector2.Zero, 0, Color.White);

            //EndShaderMode();
            

            // Draw all the Debug Windows
            
            #if DEBUG    

                

                RlImGui.Begin();

                InspectorVisual.ClickAndDrag();

                InspectorVisual.ImGUIBeh();

                RlImGui.End();

            
            #endif

            EndDrawing();
            
            
        }

        #endregion

        #if DEBUG

            RlImGui.Shutdown();

        #endif

        SpriteManager.RemoveAllTextures();


        CloseWindow();
    }


    static void UpdateGame()
    {
        //PhysicsManager.CallPhysicUpdate();
    }


    #region Scene Data
    // Used by the Serializer to Open Data files, and load its content

    /// <summary>
    /// Loads an Entity list from a .json file.
    /// </summary>
    public static void LoadScene(string path, bool relative = false)
    {
        
        var data = File.ReadAllText((relative? ProjectLoc : "") + path );

        // Clean up current entity list
        foreach (var entity in Entities.ToList())
        {
            entity.Value.Delete();
        }


        Dictionary<ulong, Entity>? ConvertedData = JsonConvert.DeserializeObject<Dictionary<ulong, Entity>>(data, settings);

        if(ConvertedData != null) 
            Entities = ConvertedData;
        else
            throw new FileLoadException("Error while loading Data. The target file might not exist, or is an unmatchable Json");
    }

    
    // Used by the Serializer to Save all data to files

    /// <summary>
    /// Saves all the Entities in a .json file.
    /// </summary>
    public static void SaveScene(string fileName, string? path = null)
    {   
        if(path == null) path = SaveLocation;
        else SaveLocation = path;

        string jsonStringGen = JsonConvert.SerializeObject(Entities, settings);

        SaveLocation = Path.EndsInDirectorySeparator(SaveLocation) ? SaveLocation : SaveLocation + "/";

        File.WriteAllText(path + "/" + fileName + ".json", jsonStringGen);
    }

    #endregion
    
    #region Entity List Helpers

    /// <summary>
    /// Retrieves the Entity with matching ID, or <c>null</c> if no Entity is found.
    /// </summary>
    public static Entity? TryFindEntity(ulong ID)
    {
        return Entities.TryGetValue(ID, out Entity? value) ? value : null;
    }

    /// <summary>
    /// Retrieves the Entity with matching ID.
    /// </summary>
    public static Entity FindEntity(ulong ID)
    {
        return Entities[ID];
    }

    /// <summary>
    /// Adds an Entity to the Entity list. Not recommended to use unless you really know what you are doing.
    /// </summary>
    public static void AddEntity(Entity entity)
    {
        Entities.Add(entity.ID, entity);
    }

    /// <summary>
    /// Removes an Entity from the Entity list. Not recommended to use unless you really know what you are doing.
    /// </summary>
    public static void RemoveEntity(ulong ID)
    {
        Entities.Remove(ID);
    }


    /// <summary>
    /// Retrieves the lowest, unoccupied ID from the Entity list.
    /// </summary>
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

    /// <summary>
    /// Returns a list containing all the Entities.
    /// </summary>
    public static List<Entity> GetAllEntities()
    {
        return [.. Entities.Values];
    }

    #endregion

    #region Renderer List Helpers
    
    /// <summary>
    /// Adds the RendererBehaviour <paramref name="rendBeh"/> to the Renderers list.
    /// </summary>
    public static void AddRenderer(RendererBehaviour rendBeh)
    {
        Renderers.Add(rendBeh);
    }

    /// <summary>
    /// Removes the RendererBehaviour <paramref name="rendBeh"/> from the Renderers list.
    /// </summary>
    public static void RemoveRenderer(RendererBehaviour rendBeh)
    {
        Renderers.Remove(rendBeh);
    }


    public static RendererBehaviour[] GetAllRenderers()
    {
        return [.. Renderers];
    }

    #endregion

    #region File management

    static void SetupFolders()
    {

        if(!Directory.Exists(ProjectLoc + "/Resources"))
            Directory.CreateDirectory(ProjectLoc + "/Resources");
    
        if(!Directory.Exists(ProjectLoc + "/Resources/Images"))
            Directory.CreateDirectory(ProjectLoc + "/Resources/Images");
    } 


    #endregion
}