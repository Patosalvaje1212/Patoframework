using System.Numerics;

namespace PF.Physics;

public struct CollisionInfo
{
    public Actor EntityA;
    public Actor EntityB;
    public Vector2 Normal;
    public float Depth;
    public Vector2 ContactPoint;

    public CollisionInfo(Actor a, Actor b, Vector2 normal, float depth, Vector2 contact)
    {
        EntityA = a;
        EntityB = b;
        Normal = normal;
        Depth = depth;
        ContactPoint = contact;
    }
}