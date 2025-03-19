using PatoframeWork.Rendering;
using Raylib_cs;

namespace PatoframeWork;

public class Behaviour
{
    [InspectorHide]
    public required Entity owner;

    public virtual void OnAdd() {}
    public virtual void OnRemove() {}
    public virtual void UpdateEffect() 
    {}
}


public static class BehaviourHelper
{
    #region Generic
    public static T AddBehaviour<T>(this Entity entity) where T : Behaviour
    {
        if(entity != null)
        {
            Behaviour? beh = Activator.CreateInstance<T>();

            if(beh != null)
            {
                beh.owner = entity;
                entity.Behaviours.Add(beh);

                beh.OnAdd();

                return (T)beh;
            }

            throw new ArgumentException("Could not find type of " + nameof(T));
        }
            

        throw new ArgumentException("Cannot add " + nameof(T) + " to an empty Entity");
    }

    public static void RemoveBehaviour(this Behaviour behaviour)
    {
        behaviour.OnRemove();
        behaviour.owner.Behaviours.Remove(behaviour);
    }

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
    
    public static RendererBehaviour SetSize(this RendererBehaviour beh, int size)
    {
        beh.Size = size;

        return beh;
    }

    public static RendererBehaviour SetOrder(this RendererBehaviour beh, int order)
    {
        beh.Order = order;

        return beh;
    }

    public static RendererBehaviour SetColor(this RendererBehaviour beh, Color color)
    {
        beh.Color = color;

        return beh;
    }

    #endregion
}