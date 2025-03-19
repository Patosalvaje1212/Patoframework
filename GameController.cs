using Raylib_cs;

using System.Numerics;
using PatoframeWork.Rendering;
using PatoframeWork;


using static Raylib_cs.Raylib;
using PatoframeWork.Physics;
using rlImGui_cs;
using ImGuiNET;
using Newtonsoft.Json;

public class GameController()
{
    public static GameController? I;
    public const bool DEBUG_MODE = true;
    public const int PhysicFrameUpdate = 20;

    
    public string? saveLocation = "./";
    public string? fileSaveName = "";

    readonly JsonSerializerSettings settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    [JsonIgnore]
    public List<RendererBehaviour> renderers = [];
    public Dictionary<ulong, Entity> entities = [];

    [JsonIgnore]
    public Action? Update;

    

    
    public int CurrentFrame;

    public void MainThread()
    {
        SetConfigFlags(ConfigFlags.ResizableWindow);
        SetConfigFlags(ConfigFlags.AlwaysRunWindow);
        SetConfigFlags(ConfigFlags.MaximizedWindow);
        
        InitWindow(1400, 700, "Hello World");
        
        MaximizeWindow();

        entities = [];
        

        CurrentFrame = 0;
        
        SetTargetFPS(60);

        Update = new(UpdateGame);



        var a = new Entity("a") { LocalPosition = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2)} ;
        a.AddBehaviour<RendererBehaviour>();

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
            CameraManager.I.UpdateCycle();
	
            var toRender = renderers.OrderBy((res) => res.Order).Where((res) => res.owner.Active).ToList();

            BeginDrawing();

            ClearBackground(Color.Beige);
            

            BeginMode2D(CameraManager.I.cam);

            
            
            for (int i = 0; i < toRender.Count; i++)
            {
                DrawCircle((int)MathF.Round(toRender[i].owner.GlobalPosition.X), (int)MathF.Round(toRender[i].owner.GlobalPosition.Y), toRender[i].Size, toRender[i].Color);
            }
            

            EndMode2D();


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

    void UpdateGame()
    {

    }

    public void LoadScene()
    {
        string jsonStringGen = String.Empty;

        var data = File.ReadAllText(saveLocation + "/" + fileSaveName);

        var ConvertedData = JsonConvert.DeserializeObject<GameController>(data, settings);

        I = ConvertedData;
    }

    public void SaveScene()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        string jsonStringGen = JsonConvert.SerializeObject(GameController.I, settings);

        File.Delete(saveLocation + "/" + fileSaveName + ".json");
        File.WriteAllText(saveLocation + "/"+ fileSaveName + ".json", jsonStringGen);
    }
}