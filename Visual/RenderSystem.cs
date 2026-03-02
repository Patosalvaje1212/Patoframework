global using Transform = PF.Physics.Transform;

using System.Numerics;
using Raylib_cs;

namespace PF.Visual;
/// <summary>
/// System with simple rendering logic
/// </summary>
public class RenderSystem : ActorSystem
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    Dictionary<Actor, Sprite> spriteMapper;
    Dictionary<Actor, Transform> transformMapper;

    Func<Actor, float> orderFunction; 

    /// <summary>
    /// Initializes a new instance of the system
    /// </summary>
    /// <param name="world"><c>PF.World</c> that created this system</param>
    /// <param name="order">Custom rendering order for target sprites</param>
    public RenderSystem(World world, Func<Actor, float>? order = null) : base(world)
    {
        AddRequiredType(typeof(Sprite), typeof(Transform));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        orderFunction = order ?? new ((res) => spriteMapper[res].Index);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void Init()
    {
        spriteMapper = world.GetMapper<Sprite>();
        transformMapper = world.GetMapper<Transform>();
    }
    
    public override void Draw(nint renderer)
    {
        List<Actor> cMActors = OrderByCriteria();

        foreach (var actor in cMActors)
        {
            // If entity is innactive, continue
            if(!actor) continue;


            var sprite = spriteMapper[actor];
            var transform = transformMapper[actor];
            
            sprite.RenderAt(transform);
        }
    }


    public override void Update(double deltaTime)
    {
        foreach (var actor in mActors)
        {
            if(!actor) continue;

            spriteMapper[actor].AdvanceTime(deltaTime);
        }
    }

    List<Actor> OrderByCriteria()
    {
        return [.. mActors.OrderBy(res => orderFunction(res))];
    }
}