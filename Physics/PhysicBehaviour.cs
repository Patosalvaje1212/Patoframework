using PatoframeWork;
using Raylib_cs;
using System.Numerics;

namespace PatoframeWork.Physics;

public class PhysicBehaviour : Behaviour
{
    public Vector2 Velocity = Vector2.Zero;

    public override void UpdateEffect()
    {

        Owner.GlobalPosition += Velocity;

        if(GameController.CurrentFrame % GameController.PhysicFrameUpdate == 0)
        {
            PhysicsUpdate();
        }
    }

    public virtual void PhysicsUpdate()
    {
        
    }

    public override void OnAdd()
    {

    }

    public override void OnRemove()
    {
        //
    }
}