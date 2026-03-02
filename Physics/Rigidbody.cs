using System.Numerics;

namespace PF.Physics;

public class Rigidbody
{
    public float Mass;

    public Vector2 velocity;
    public Vector2 Velocity
    {
        get => velocity;
        set
        {
            velocity = value;
            //Logger.Log(value.ToString());
        }
    }
    public Vector2 Force;
    public readonly BodyType Type;
    public float Restitution; // Bounciness
    public float Friction;
    public bool IsAwake;

    public float GravMult;

    public readonly List<CollisionInfo> collisions = [];

    public Rigidbody(float mass = 1.0f, BodyType type = BodyType.Dynamic)
    {
        Mass = mass;
        Velocity = -Vector2.UnitY * 5;
        Force = Vector2.Zero;
        Type = type;
        Restitution = 0.2f;
        Friction = 0.4f;
        IsAwake = true;

        GravMult = 1f;

    }

    public void JoinForce(float x, float y)
    {
        Force = new(x != 0f ? x : Force.X, y != 0f ? y : Force.Y);
    }

    public enum BodyType
    {
        Dynamic,    // Moves under physics
        Static,     // Doesn't move, infinite mass
        Kinematic   // Moves under code control, doesn't respond to forces
    }
}

