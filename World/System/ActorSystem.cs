namespace PF;

/// <summary>
/// Base class for all Systems in the ECS that interact with Actors.
/// </summary>
public abstract class ActorSystem : SystemBase
{
    /// <summary>
    /// List of all Actors that match the Component criteria.
    /// </summary>
    protected HashSet<Actor> mActors = [];

    /// <summary>
    /// Reference to the <c>World</c> that owns this system.
    /// </summary>
    protected World world;

    private HashSet<Type> allOf = [];
    private HashSet<Type> someOf = [];
    private HashSet<Type> noneOf = [];


    public abstract void Init();
    public abstract void Draw(nint renderer);
    public abstract void Update(double deltaTime);


    public ActorSystem(World world)
    {
        this.world = world;
    }

    /// <summary>
    /// Notifies this system that an Actor changed components, and checks if it should be added or removed from the matching actors list. <br/>
    /// Called internally when adding or deleting Actors, or it's components.
    /// </summary>
    /// <param name="actorID"></param>
    public virtual void ReloadActor(Actor actorID)
    {
        foreach (var type in noneOf)
        {
            if (world.TryGetMapper(type, out BehaviourMap? val) && val?.Contains(actorID) == true)
            {
                mActors.Remove(actorID);
                return;
            }
        }

        foreach (var type in allOf)
        {
            if (!world.TryGetMapper(type, out BehaviourMap? val) || !val?.Contains(actorID) == true)
            {
                mActors.Remove(actorID);
                return;
            }
        }

        if (someOf.Count == 0)
        {
            mActors.Add(actorID);
        }

        foreach (var type in someOf)
        {
            if (world.TryGetMapper(type, out BehaviourMap? val) && val?.Contains(actorID) == true)
            {
                mActors.Add(actorID);
                return;
            }
        }
    }

    /// <summary>
    /// Adds component criteria. An entity must have all components in this list. 
    /// </summary>
    /// <param name="types">The components the entity must have</param>
    protected void AddRequiredType(params Type[] types)
    {
        foreach (var type in types)
        {
            allOf.Add(type);
        }
    }

    /// <summary>
    /// Adds component criteria. An entity must have at least one of the components in this list.
    /// </summary>
    /// <param name="types">The components the entity must have at least one of</param>
    protected void AddBeInType(params Type[] types)
    {
        foreach (var type in types)
        {
            someOf.Add(type);
        }
    }

    /// <summary>
    /// Adds component criteria. An entity must have none of the components in this list.
    /// </summary>
    /// <param name="types">The components the entity must not have</param>
    protected void AddExcludeType(params Type[] types)
    {
        foreach (var type in types)
        {
            noneOf.Add(type);
        }
    }
}