using Raylib_cs;

using System.Numerics;
using PatoframeWork.Rendering;


using static Raylib_cs.Raylib;
using rlImGui_cs;
using ImGuiNET;
using Newtonsoft.Json;


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

    public static LightBehaviour[] lights = [];



    // Is called once per frame. Used to call Entity.SelfUpdate()
    [JsonIgnore]
    public static Action? Update;

    

    // Frame Counter 
    public static int CurrentFrame { get; private set; } = 0;



    // Rend--
    
    struct Light()
    {
        public int enabled = 1;
        public Vector2 position = Vector2.Zero;
    }

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

        SpriteManager.LoadTextureFolder("Resources/Images");


        var newO = new Entity("ABc");
        newO.AddBehaviour<RendererBehaviour>().SetColor(Color.Blue).SetSize(40);


    


        // Only edit mode Setups
        #if DEBUG

            CameraManager.I.freeRoam = true;

            RlImGui.Setup(true);    
        
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;


        #endif

        
        
        // Shader Setup:
        

        

        
        #region Default Shader Setup

        bool firstLoad = true;

        Shader PixelateShader = LoadShader( null, "Rendering/Shaders/PixelateScreen.fs");

        int pixelteLoc = GetShaderLocation(PixelateShader, "DownscaleRes");

        float pixelateAmount = 5;


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

        #endregion


        RenderTexture2D texture2D = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());

        #region GameLoop

        while (!WindowShouldClose())
        {
    
            texture2D.Texture.Width = GetScreenWidth();
            texture2D.Texture.Height = GetScreenHeight();
            CurrentFrame ++;

            Update.Invoke();

            //mult[0] = ((CurrentFrame % 10) + 1)  / 11f;
            //mult[1] = ((CurrentFrame % 50) + 1)  / 51f;
            //mult[2] = ((CurrentFrame % 100) + 1) / 101f;







            // Update Camera
            CameraManager.I.UpdateCamera();


            if(SpriteManager.IsDirty)
            {
                SpriteManager.LoadAllTextures();
            }



            if(firstLoad)
            {
                SetShaderValue(PixelateShader, pixelteLoc, new Vector2(GetScreenWidth(), GetScreenHeight()) / pixelateAmount, ShaderUniformDataType.Vec2);
            }

            // Load Shader Variables
            if(LightsManager.IsDirty() || firstLoad)
            {
                var LightPoses = LightsManager.GetNearestLights(5);

                var LightColors = LightsManager.GetNearestLightsColors();

                Console.WriteLine(LightPoses.Length);
            
                SetShaderValue(DefaultShader, lNumberLoc, LightPoses.Length, ShaderUniformDataType.Int);
                //SetShaderValueV(DefaultShader, lLoc, LightPoses, ShaderUniformDataType.Vec3, 4);

                SetShaderValueV(DefaultShader, lightPosesLoc, LightPoses, ShaderUniformDataType.Vec3, LightPoses.Length);
                
                SetShaderValue(DefaultShader, rLoc, new float[2] { GetScreenWidth(), GetScreenHeight()}, ShaderUniformDataType.Vec2);
                SetShaderValue(DefaultShader, camOffLoc, CameraManager.I.cam.Target, ShaderUniformDataType.Vec2);
                SetShaderValue(DefaultShader, camZoomLoc, CameraManager.I.cam.Zoom, ShaderUniformDataType.Float);


                SetShaderValue(DefaultShader, lrLoc, LightsManager.LightResolution, ShaderUniformDataType.Int);

                SetShaderValue(DefaultShader, fallOffLoc, LightsManager.LightFallOff, ShaderUniformDataType.Vec3);

                Vector4 ambientLight = ColorNormalize(LightsManager.AmbientLight);


                SetShaderValueV(DefaultShader, lightColorLoc, LightColors, ShaderUniformDataType.Vec4, LightPoses.Length);
                SetShaderValue(DefaultShader, ambientColorLoc, ambientLight, ShaderUniformDataType.Vec4 );


                firstLoad = false;
            }

        
            

            var toRender = Renderers.Where((res) => res.Owner.Active).OrderBy((res) => res.Order).ToList();

            BeginTextureMode(texture2D);
            
            BeginMode2D(CameraManager.I.cam);
            
            ClearBackground(Color.Beige);
            
            
            BeginShaderMode(DefaultShader);
        

                for (int i = 0; i < toRender.Count; i++)
                {

                    if(toRender[i].RenderType == RendererBehaviour.ShapeType.Image)
                    {            
                        if(SpriteManager.LoadedImages.TryGetValue(toRender[i].ImageID, out ImageData? image))
                        {
                            image.loadedTexture ??= LoadTextureFromImage(image.image);
                            

                            if(image.loadedNormal == null)
                            {
                                if(image.imageNormal != null) image.loadedNormal = LoadTextureFromImage((Image)image.imageNormal);
                                else image.loadedNormal = SpriteManager.DefaultText;
                            }



                            SetShaderValueTexture(DefaultShader, normalMapLoc, (Texture2D)image.loadedNormal);

                            if(image.SpriteRects.TryGetValue(toRender[i].SpriteID, out Rectangle sprite) && image.loadedTexture is Texture2D loadedTexture)
                            {
                                DrawTexturePro(loadedTexture, sprite, new Rectangle(toRender[i].Owner.LocalPosition - toRender[i].Owner.GlobalPosition, Raymath.Vector2Normalize(sprite.Size) * toRender[i].Size), toRender[i].Owner.LocalPosition, toRender[i].zRot, toRender[i].Color);
                            }
                            else
                            ErrorManager.LogError($"Unexpected missing texture when loading texure ID {toRender[i].ImageID}");
                            
                        } else
                        ErrorManager.LogError($"Could not find Image ID {toRender[i].ImageID}");

                    } else if(toRender[i].RenderType == RendererBehaviour.ShapeType.Square)

                    {
                        SetShaderValueTexture(DefaultShader, normalMapLoc, (Texture2D)SpriteManager.DefaultText);

                        DrawRectangle((int)MathF.Round(toRender[i].Owner.GlobalPosition.X) - (int)toRender[i].Size/2, (int)MathF.Round(toRender[i].Owner.GlobalPosition.Y) - (int)toRender[i].Size/2, (int)toRender[i].Size, (int)toRender[i].Size, toRender[i].Color);

                    } else if(toRender[i].RenderType == RendererBehaviour.ShapeType.Circle)
                    {
                        SetShaderValueTexture(DefaultShader, normalMapLoc, (Texture2D)SpriteManager.DefaultText);
                     
                        DrawCircle((int)MathF.Round(toRender[i].Owner.GlobalPosition.X), (int)MathF.Round(toRender[i].Owner.GlobalPosition.Y), toRender[i].Size, toRender[i].Color);
                    }

                }
                
            EndShaderMode();
            EndMode2D();
            
            EndTextureMode();
            

            BeginDrawing();
            BeginShaderMode(PixelateShader);

                DrawTexturePro(texture2D.Texture, new Rectangle(0, 0, texture2D.Texture.Width, texture2D.Texture.Height), new Rectangle(0, 0, GetScreenWidth(), GetScreenHeight()), Vector2.Zero, 0, Color.White);

            EndShaderMode();

            // Draw all the Debug Windows
            
            #if DEBUG    

                RlImGui.Begin();

                InspectorVisual.ImGUIBeh();

                RlImGui.End();
            
            #endif

            EndDrawing();
            
            
        }

        #endregion

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

        if(ConvertedData != null) 
            Entities = ConvertedData;
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