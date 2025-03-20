using PatoframeWork.Rendering;
using Raylib_cs;

namespace PatoframeWork;

/// <summary>
/// Basic class for all the Behaviours
/// </summary>
public class Behaviour
{

    /// <summary>
    /// Entity this behaviour is attached to
    /// </summary>
    [InspectorHide]
    public required Entity Owner;
    /// <summary>
    /// Called once when the Behaviour is added to an Entity
    /// </summary>
    public virtual void OnAdd() {}

    /// <summary>
    /// Called once when the Behaviour is removed from an Entity
    /// </summary>
    public virtual void OnRemove() {}

    /// <summary>
    /// Called once each frame, as long as the Owner Entity has the variable ReceiveUpdates enabled
    /// </summary>
    public virtual void UpdateEffect() {}
}

/// <summary>
/// Static class for general helpers
/// </summary>
public static class BehaviourHelper
{
    #region Generic

    /// <summary>
    /// Adds the type of Behaviour to a target entity
    /// </summary>
    /// <returns>
    /// The added Behaviour
    /// </returns>
    public static T AddBehaviour<T>(this Entity entity) where T : Behaviour
    {
        if(entity != null)
        {
            Behaviour? beh = Activator.CreateInstance<T>();

            if(beh != null)
            {
                beh.Owner = entity;
                entity.Behaviours.Add(beh);

                beh.OnAdd();

                return (T)beh;
            }

            throw new ArgumentException("Could not find type of " + nameof(T));
        }
            

        throw new ArgumentException("Cannot add " + nameof(T) + " to an empty Entity");
    }

    /// <summary>
    /// Removes a target Behaviour from an entity
    /// </summary>
    public static void RemoveBehaviour(this Behaviour behaviour)
    {
        behaviour.OnRemove();
        behaviour.Owner.Behaviours.Remove(behaviour);
    }

    /// <summary>
    /// Finds a Behaviour in a target entity
    /// </summary>
    /// <returns>
    /// The first instance found of the specified behaviour
    /// </returns>
    public static T? FindBehaviour<T>(this Entity entity) where T : Behaviour
    {
        for (int i = 0; i < entity.Behaviours.Count; i++)
        {
            if(entity.Behaviours[i].GetType() == typeof(T))
                return (T)entity.Behaviours[i];
        }

        return null;
    }
    #endregion


    #region RendererBehaviour
    
    /// <summary>
    /// Sets the size (render size) of a RenderBehaviour
    /// </summary>
    /// <returns>
    /// The target RenderBehaviour
    /// </returns>
    public static RendererBehaviour SetSize(this RendererBehaviour beh, int size)
    {
        beh.Size = size;

        return beh;
    }

    /// <summary>
    /// Sets the render order of a RenderBehaviour
    /// </summary>
    /// <returns>
    /// The target RenderBehaviour
    /// </returns>
    public static RendererBehaviour SetOrder(this RendererBehaviour beh, int order)
    {
        beh.Order = order;

        return beh;
    }

    /// <summary>
    /// Sets the Color of a RenderBehaviour
    /// </summary>
    /// <returns>
    /// The target Behaviour
    /// </returns>
    public static RendererBehaviour SetColor(this RendererBehaviour beh, Color color)
    {
        beh.Color = color;

        return beh;
    }

    #endregion
}