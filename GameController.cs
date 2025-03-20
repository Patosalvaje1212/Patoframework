using Raylib_cs;

using System.Numerics;
using PatoframeWork.Rendering;
using PatoframeWork;


using static Raylib_cs.Raylib;
using PatoframeWork.Physics;
using rlImGui_cs;
using ImGuiNET;
using Newtonsoft.Json;


namespace PatoframeWork;
public class GameController()
{
    // Singleton : Static property referencing Instance data
    public static GameController I = new();

    // Amount of frames for each PhysicUpdate
    public const int PhysicFrameUpdate = 20;

    // Save Location and Filename -- They get initialized after selecting a path
    public string? SaveLocation = "./";
    public string? fileSaveName = "";


    // Properties for the save system
    // NOTE: Json saves get too heavy, set Formatting to Formatting.None
    readonly JsonSerializerSettings settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    // Every object that has to render
    [JsonIgnore]
    public List<RendererBehaviour> Renderers = [];

    // Dictionary binding every Entity to an Id
    public Dictionary<ulong, Entity> Entities = [];


    // Is called once per frame. Used to call Entity.SelfUpdate()
    [JsonIgnore]
    public Action? Update;

    

    // Frame Counter 
    public int CurrentFrame;

    public void MainThread()
    {
        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetConfigFlags(ConfigFlags.AlwaysRunWindow);
        SetConfigFlags(ConfigFlags.MaximizedWindow);
        
        InitWindow(1400, 700, "Hello World");
        
        MaximizeWindow();

        Entities = [];
        

        CurrentFrame = 0;
        
        SetTargetFPS(60);

        Update = new(UpdateGame);

        
        // ----- Test Objects
        // TODO: Remplace them with Add Entity Button in GUI
        var a = new Entity("a") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2)} ;
        a.AddBehaviour<RendererBehaviour>().SetColor(Color.LightGray);

        a = new Entity("b") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2) + Vector2.UnitX * 30} ;
        a.AddBehaviour<RendererBehaviour>().Size = 25;


        var b = new Entity("c") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2) + Vector2.UnitX * 60};
        b.AddBehaviour<PhysicBehaviour>().Velocity = Vector2.One * 2f;
        b.AddBehaviour<RendererBehaviour>();
        b.ReceiveUpdates = true;

        a = new Entity("d") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2) + Vector2.UnitX * 90} ;
        a.AddBehaviour<RendererBehaviour>().Color = Color.Brown;
        a.Active = false;

        a = new Entity("e") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2) + Vector2.UnitX * 120} ;
        a.AddBehaviour<RendererBehaviour>().Color = Color.Green;

        a.SetParent(b);

        a = new Entity("f") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2) + Vector2.UnitX * 120} ;
        a.AddBehaviour<RendererBehaviour>().Color = Color.Green;

        a.SetParent(b);

        var c = new Entity("g") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2) + Vector2.UnitX * 120} ;
        c.AddBehaviour<RendererBehaviour>().Color = Color.Green;

        c.SetParent(a);

        // ----

        

       

        // Only edit mode Setups
        #if DEBUG

            CameraManager.I.freeRoam = true;

            RlImGui.Setup(true);    
        
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;


        #endif
        

        Texture2D test = LoadTexture("test.png");
        int frames = 20;
        Image test2 = LoadImageAnim("PlayerAnim.png", out frames);
        Texture2D test2L = LoadTextureFromImage(test2);

        Console.WriteLine(frames);

        while (!WindowShouldClose())
        {
            CurrentFrame ++;

            Update.Invoke();
            // Update Camera
            CameraManager.I.UpdateCamera();
	
            // Filter renderers && order them by draw Order
            var toRender = Renderers.Where((res) => res.Owner.Active).OrderBy((res) => res.Order).ToList();

            BeginDrawing();

            ClearBackground(Color.Beige);
            

            BeginMode2D(CameraManager.I.cam);

            for (int i = 0; i < toRender.Count; i++)
            {
                if(i == 0) DrawTexturePro(test2L, new Rectangle(Vector2.Zero, new Vector2(48, 32)), new Rectangle(Vector2.One, Vector2.One * 50), toRender[i].Owner.GlobalPosition, toRender[i].zRot, toRender[i].Color);
                else
                DrawCircle((int)MathF.Round(toRender[i].Owner.GlobalPosition.X), (int)MathF.Round(toRender[i].Owner.GlobalPosition.Y), toRender[i].Size, toRender[i].Color);
            }
            

            EndMode2D();

            // Draw all the Debug Windows
            #if DEBUG

                RlImGui.Begin();

                InspectorVisual.ImGUIBeh();

                RlImGui.End();
            
            #endif
            
            EndDrawing();
            
        }

        #if DEBUG

            RlImGui.Shutdown();

        #endif

        CloseWindow();
    }

    void UpdateGame() {}


    // Used by the Serializer to Open Data files, and load its content
    public void LoadScene()
    {
        var data = File.ReadAllText(SaveLocation + fileSaveName);

        var ConvertedData = JsonConvert.DeserializeObject<GameController>(data, settings);

        if(ConvertedData != null) I = ConvertedData;
        else throw new FileLoadException("Error while loading Data. The target file might not exist, or is an unmatchable Json");
    }


    // Used by the Serializer to Save all data to files
    public void SaveScene()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        string jsonStringGen = JsonConvert.SerializeObject(GameController.I, settings);

        File.Delete(SaveLocation + fileSaveName);
        File.WriteAllText(SaveLocation + fileSaveName, jsonStringGen);
    }
}