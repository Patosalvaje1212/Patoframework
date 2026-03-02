using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using Raylib_cs;

namespace PF;

/// <summary>
/// Class that manages and contains all systems and actors inside a game.
/// </summary>
/// <remarks>
/// Multiple Worlds may be instantiated at a time, but their updating and drawing order should be handled manually ( don't call <c>WorldStep</c>)
/// </remarks>
public class World
{
    private List<(SystemBase, int)> systems = [];
    private HashSet<Actor> actors = [];
    
    private BehaviourMapManager mapManager;
    private nint renderer;

    private Stopwatch stopwatch;

    bool inited;

    /// <summary>
    /// List of working systems and their priorities. Returns a copy ordered by priority if the current <c>World</c> is already initialized. 
    /// </summary>
    public List<(SystemBase, int)> Systems => inited ? [.. systems] : systems;

    public World(nint renderer)
    {
        mapManager = new BehaviourMapManager();
        this.renderer = renderer;

        stopwatch = new();
    }

    /// <summary>
    /// Adds a system to the working system list with the specified priority. Lower numbers are executed first.
    /// </summary>
    /// <remarks>
    /// If multiple systems have the same priority, their <c>Update</c> are executed at the same time (different threads). <br/>
    /// All Draw calls are executed secuentially, if multiple systems have the same priority their <c>Draw</c> are executed in a random (consisten) order.
    /// </remarks>
    /// <param name="target">The system to add</param>
    /// <param name="priority">The execution priority of this system</param>
    public void AddSystem(SystemBase target, int priority)
    {
        systems.Add((target, priority));
    }

    /// <summary>
    /// Retrieves a Mapper of a type, and creates one if it is not found
    /// </summary>
    /// <typeparam name="T">Type of mapper to retrieve</typeparam>
    /// <returns>A Dictionary containing all actors with a component of type <c>T</c> as keys and said component as values.</returns>
    public Dictionary<Actor, T> GetMapper<T>() where T : class
    {
        mapManager.GetMapper<T>(out IDictionary dict);

        return (Dictionary<Actor, T>)dict;
    }
    

    /// <summary>
    /// Tries retrieving a mapper of a type.
    /// </summary>
    /// <param name="type">Type of mapper to retrieve</param>
    /// <param name="mapper">Retrieved mapped, if found</param>
    /// <returns><c>true</c> if a mapper of the specified type is found and <c>false</c> otherwise.</returns>
    public bool TryGetMapper(Type type, out BehaviourMap? mapper)
    {

        if(mapManager.mappers.ContainsKey(type))
        {
            mapper = mapManager.mappers[type];
            return true;
        }

        mapper = null;
        return false;
    }


    /// <summary>
    /// Initializes all working systems, and reloads all of their matching actor lists. Call this once, right before entering on the game loop.
    /// </summary>
    public void Init()
    {
        stopwatch.Restart();

        Logger.Log("Initializing " + systems.Count + " systems.");

        systems = [.. systems.OrderBy(res => res.Item2)];

        foreach (var system in systems)
        {
            if (system.Item1 is ActorSystem aSystem)
            {
                foreach (var actorID in actors)
                    aSystem.ReloadActor(actorID);
            }

        }

        foreach (var system in systems)
            system.Item1.Init();

        inited = true;
    }


    /// <summary>
    /// Creates an Actor and stores it.
    /// </summary>
    /// <returns>Created actor.</returns>
    public Actor CreateActor()
    {
        Actor newActor = new Actor();
        actors.Add(newActor);

        return newActor;
    }

    /// <summary>
    /// Reloads all actor systems' matching actor lists. This method is resource intensive and should not be called in an <c>Update</c>.
    /// </summary>
    /// <param name="actor"></param>
    private void ReloadAllActorSystems(Actor actor)
    {
        IEnumerable<ActorSystem> aSystems = systems.Where((res) => res.Item1 is ActorSystem).Cast<ActorSystem>();
        foreach (var system in aSystems)
        {
            system.ReloadActor(actor);
        }
    }

    /// <summary>
    /// Removes an Actor from the world. And reloads all corresponding systems.
    /// </summary>
    /// <param name="actor">Actor to remove</param>
    public void RemoveActor(Actor actor)
    {
        Logger.Log("Deleting Actor : " + actor);
        foreach (var mapper in mapManager.mappers)
        {
            mapper.Value.GetMapper().Remove(actor);
        }

        ReloadAllActorSystems(actor);
    }

    /// <summary>
    /// Adds a component of a type to an Actor, and reloads all systems.<br/>
    /// An Actor can only contain one element of each type, and subsequent calls of this method will remplace the original component.
    /// </summary>
    /// <typeparam name="T">Type of the Component to add</typeparam>
    /// <param name="actor">Actor to add the component to</param>
    /// <param name="data">Component to add</param>
    public void AddComponent<T>(Actor actor, T data) where T : class
    {
        GetMapper<T>().Add(actor, data);

        ReloadAllActorSystems(actor);
    }

    /// <summary>
    /// Removes a component of a type from an Actor,,and reloads all systems.<br/>
    /// </summary>
    /// <typeparam name="T">Type of the component to remove</typeparam>
    /// <param name="actor">Actor to remove the component from</param>
    /// <returns><c>true</c> if the target component was removed, <c>false</c> otherwise</returns>
    public bool RemoveComponent<T>(Actor actor) where T : class
    {
        if (!TryGetMapper(typeof(T), out BehaviourMap? mapper) || !mapper?.Contains(actor) == true)
        {
            return false;
        }


        mapper?.GetMapper().Remove(actor);

        ReloadAllActorSystems(actor);

        return true;
    }

    /// <summary>
    /// Calls <c>SystemBase.Draw</c> method in all working systems.
    /// </summary>
    /// <seealso cref="SystemBase.Draw(nint)"/>
    void Draw()
    {
        foreach (var system in systems)
        {
            system.Item1.Draw(renderer);
        }
    }

    /// <summary>
    /// Calls <c>SystemBase.Update</c> method in all working systems, with the correct delta.
    /// </summary>
    /// <seealso cref="SystemBase.Update(double)"/>
    void Update()
    {
        double delta = stopwatch.Elapsed.TotalSeconds;
        stopwatch.Restart();

        int l = systems[0].Item2 - 1;
        
        for (int i = 0; i < systems.Count; i++)
        {
            var system = systems[i];

            if((i != systems.Count - 1 && system.Item2 == systems[i + 1].Item2) || l == system.Item2)
            {
                l = system.Item2;
                
                ThreadPool.QueueUserWorkItem((res) => system.Item1.Update(delta));
            } else
            {
                system.Item1.Update(delta);
            }
        }
    }

    /// <summary>
    /// Calls <c>Update</c> and <c>Draw</c> after
    /// </summary>
    public void WorldStep(Color? color = null)
    {
        Update();


        Raylib.BeginDrawing();
        Raylib.ClearBackground(color ?? Color.Beige);
        Draw();
        Raylib.EndDrawing();
    }

    /// <summary>
    /// Window initializion for SDL
    /// </summary>
    /// <param name="title">Text to display as the window name</param>
    /// <param name="initWidth">Initial width of the window (in pixels)</param>
    /// <param name="initHeight">Initial height of the window (in pixels)</param>
    /// <param name="window">Outs a pointer to the created SDL Window</param>
    /// <param name="renderer">Outs a pointer to the asociated SDL Renderer</param>
    /// <param name="fullscreen">Marks if the window should start in fullscreen</param>
    /// <exception cref="Exception">Signals an SDL Error when initializating or creating a window</exception>
    public static void Setup(string title, int initWidth, int initHeight, bool fullscreen = false)
    {
        
        Raylib.InitWindow(initWidth, initHeight, title);

        if(fullscreen)
            Raylib.ToggleFullscreen();
    }   
}