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
public static class GameController
{
    // Amount of frames for each PhysicUpdate
    public const int PhysicFrameUpdate = 20;

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
    public static List<RendererBehaviour> Renderers = [];

    // Dictionary binding every Entity to an Id
    public static Dictionary<ulong, Entity> Entities = [];


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
	
            // Filter renderers && order them by draw Order
            var toRender = Renderers.Where((res) => res.Owner.Active).OrderBy((res) => res.Order).ToList();



            if(IsKeyPressed(KeyboardKey.Minus)) Entities[1].Behaviours[0].CloneBehaviour();
            if(IsKeyPressed(KeyboardKey.M)) Entities[1].Duplicate();
            

            BeginDrawing();

            ClearBackground(Color.Beige);
            

            BeginMode2D(CameraManager.I.cam);


            if(true)
            {
            }


            for (int i = 0; i < toRender.Count; i++)
            {
                DrawCircle((int)MathF.Round(toRender[i].Owner.GlobalPosition.X), (int)MathF.Round(toRender[i].Owner.GlobalPosition.Y), toRender[i].Size, toRender[i].Color);
            }
            

            
            // Draw all the Debug Windows
            #if DEBUG
    
    
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

        CloseWindow();
    }

    static void UpdateGame() {}


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
}