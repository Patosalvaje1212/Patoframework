using PatoFramework;
using Raylib_cs;
using System.Numerics;

namespace PatoFramework.Physics;

public class PhysicBehaviour : Behaviour
{
    /// <summary>
    /// Moving velocity. Applied each frame to its Owner.
    /// </summary>
    public Vector2 Velocity = Vector2.Zero;

    public override void UpdateEffect()
    {
        Owner.GlobalPosition += Velocity;
    }
}