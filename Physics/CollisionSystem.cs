using System.Numerics;

namespace PF.Physics;

public class CollisionSystem : ActorSystem
{
    private HashSet<CollisionInfo> collisions = new HashSet<CollisionInfo>();
    public CollisionInfo[] Collisions => [.. collisions];
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    Dictionary<Actor, Shape> shapeMapper;
    Dictionary<Actor, Rigidbody> rigidbodyMapper;
    Dictionary<Actor, Transform> transformMapper;

    public Func<CollisionInfo, bool> BeforeCollision = new((res) => true);
    public Action<CollisionInfo> AfterCollision = new((res) => { });

    public CollisionSystem(World world) : base(world)
    {
        AddRequiredType(typeof(Shape), typeof(Rigidbody), typeof(Transform));
    }
#pragma warning restore CS8618 

    public override void Init()
    {
        shapeMapper = world.GetMapper<Shape>();
        rigidbodyMapper = world.GetMapper<Rigidbody>();
        transformMapper = world.GetMapper<Transform>();
    }


    public override void Update(double deltaTime)
    {
        foreach (var collision in collisions)
        {
            rigidbodyMapper[collision.EntityA].collisions.Clear();
            rigidbodyMapper[collision.EntityB].collisions.Clear();
        }

        collisions.Clear();

        List<Actor> actors = [.. mActors];

        // Broad phase: Check AABB overlap
        for (int i = 0; i < mActors.Count; i++)
        {
            if(!actors[i]) continue;

            Actor entityA = actors[i];
            Rigidbody rbA = rigidbodyMapper[entityA];

            for (int j = i + 1; j < mActors.Count; j++)
            {
                if(!actors[j]) continue;

                Actor entityB = actors[j];
                Rigidbody rbB = rigidbodyMapper[entityB];

                // Skip if they are the same
                if (rbA == rbB)
                    continue;
                // Skip if both are static
                if (rbA.Type == Rigidbody.BodyType.Static && rbB.Type == Rigidbody.BodyType.Static)
                    continue;

                if (CheckCollision(entityA, entityB, out var collision)
                && !collisions.Any(
                    (res) =>
                        (res.EntityA == entityA
                        && res.EntityB == entityB)
                        || (res.EntityB == entityA
                        && res.EntityA == entityB)))
                {
                    collisions.Add(collision);
                    rbA.collisions.Add(collision);
                    rbB.collisions.Add(collision);
                }
            }

        }

        // Narrow phase: Resolve collisions
        foreach (CollisionInfo collision in collisions)
        {
            Delegate[] beforeList = BeforeCollision.GetInvocationList();

            bool doCollision = true;
            for (int i = 0; i < beforeList.Length; i++)
            {
                if (!((Func<CollisionInfo, bool>)beforeList[i]).Invoke(collision))
                    doCollision = false;
            }


            if (doCollision)
            {
                ResolveCollision(collision);
                AfterCollision.Invoke(collision);
            }

        }
    }

    private bool CheckCollision(Actor a, Actor b, out CollisionInfo collision)
    {
        collision = default;

        var transformA = transformMapper[a];
        var shapeA = shapeMapper[a];
        var transformB = transformMapper[b];
        var shapeB = shapeMapper[b];

        // Circle-Circle collision
        if (shapeA.Type == Shape.ShapeType.Circle && shapeB.Type == Shape.ShapeType.Circle)
        {
            return CheckCircleCircle(a, b, transformA, shapeA, transformB, shapeB, out collision);
        }
        // Circle-Rectangle collision
        else if (shapeA.Type == Shape.ShapeType.Circle && shapeB.Type == Shape.ShapeType.Rectangle)
        {
            bool hasCollision = CheckCircleRectangle(a, b, transformB, shapeB, transformA, shapeA, out collision);
            if (hasCollision)
            {
                // Swap entities back and reverse normal
                collision = new CollisionInfo(a, b, -collision.Normal, collision.Depth, collision.ContactPoint);

            }
            return hasCollision;
        }
        // Rectangle-Circle collision
        else if (shapeA.Type == Shape.ShapeType.Rectangle && shapeB.Type == Shape.ShapeType.Circle)
        {
            // Swap entities and then fix the collision normal
            bool hasCollision = CheckCircleRectangle(b, a, transformB, shapeB, transformA, shapeA, out collision);
            if (hasCollision)
            {
                // Swap entities back and reverse normal
                collision = new CollisionInfo(a, b, -collision.Normal, collision.Depth, collision.ContactPoint);

            }
            return hasCollision;
        }
        // Rectangle-Rectangle collision
        else if (shapeA.Type == Shape.ShapeType.Rectangle && shapeB.Type == Shape.ShapeType.Rectangle)
        {
            return CheckRectangleRectangle(a, b, transformA, shapeA, transformB, shapeB, out collision);
        }

        return false;
    }

    private bool CheckCircleCircle(Actor a, Actor b, Transform tA, Shape sA,
                                   Transform tB, Shape sB, out CollisionInfo collision)
    {
        collision = default;
        float radiusA = sA.Size.X;
        float radiusB = sB.Size.X;

        Vector2 delta = tB.Position - tA.Position;
        float distance = delta.Length();
        float minDistance = radiusA + radiusB;

        if (distance < minDistance)
        {
            Vector2 normal = distance > 0 ? Vector2.Normalize(delta) : new Vector2(1, 0);
            float depth = minDistance - distance;
            Vector2 contact = tA.Position + normal * radiusA;

            collision = new CollisionInfo(a, b, normal, depth, contact);
            return true;
        }

        return false;
    }

    private bool CheckCircleRectangle(Actor circleEntity, Actor rectEntity,
                                      Transform circleTransform, Shape circleShape,
                                      Transform rectTransform, Shape rectShape,
                                      out CollisionInfo collision)
    {
        collision = default;
        float radius = circleShape.Size.X;

        // Transform circle position to rectangle's local space
        Vector2 delta = circleTransform.Position - rectTransform.Position;

        // Rotate delta by negative rectangle rotation
        float cos = MathF.Cos(-rectTransform.Rotation.Z);
        float sin = MathF.Sin(-rectTransform.Rotation.Z);
        Vector2 localDelta = new Vector2(
            delta.X * cos - delta.Y * sin,
            delta.X * sin + delta.Y * cos
        );

        // Find closest point on rectangle to circle center
        Vector2 halfExtents = rectShape.Size / 2;
        Vector2 closest = new Vector2(
            Math.Clamp(localDelta.X, -halfExtents.X, halfExtents.X),
            Math.Clamp(localDelta.Y, -halfExtents.Y, halfExtents.Y)
        );

        // Convert back to world space
        cos = MathF.Cos(rectTransform.Rotation.Z);
        sin = MathF.Sin(rectTransform.Rotation.Z);
        Vector2 worldClosest = rectTransform.Position + new Vector2(
            closest.X * cos - closest.Y * sin,
            closest.X * sin + closest.Y * cos
        );

        Vector2 circleToClosest = worldClosest - circleTransform.Position;
        float distance = circleToClosest.Length();

        if (distance < radius)
        {
            Vector2 normal = distance > 0 ? Vector2.Normalize(circleToClosest) : new Vector2(1, 0);
            float depth = radius - distance;
            Vector2 contact = worldClosest;

            collision = new CollisionInfo(circleEntity, rectEntity, normal, depth, contact);
            return true;
        }

        return false;
    }

    private bool CheckRectangleRectangle(Actor a, Actor b, Transform tA, Shape sA,
                                         Transform tB, Shape sB, out CollisionInfo collision)
    {
        collision = default;

        // Simple AABB check (ignoring rotation for simplicity)
        // For a complete implementation with rotation, we'd need Separating Axis Theorem (SAT)

        Vector2 minA = tA.Position - sA.Size / 2;
        Vector2 maxA = tA.Position + sA.Size / 2;
        Vector2 minB = tB.Position - sB.Size / 2;
        Vector2 maxB = tB.Position + sB.Size / 2;

        if (maxA.X > minB.X && minA.X < maxB.X &&
            maxA.Y > minB.Y && minA.Y < maxB.Y)
        {
            // Calculate overlap
            float overlapX = Math.Min(maxA.X - minB.X, maxB.X - minA.X);
            float overlapY = Math.Min(maxA.Y - minB.Y, maxB.Y - minA.Y);

            Vector2 normal;
            float depth;

            if (overlapX < overlapY)
            {
                normal = new Vector2(minA.X < minB.X ? 1 : -1, 0);
                depth = overlapX;
            }
            else
            {
                normal = new Vector2(0, minA.Y < minB.Y ? 1 : -1);
                depth = overlapY;
            }

            // Calculate contact point (midpoint of overlap)
            float contactX = Math.Max(minA.X, minB.X) + Math.Abs(maxA.X - minB.X) / 2;
            float contactY = Math.Max(minA.Y, minB.Y) + Math.Abs(maxA.Y - minB.Y) / 2;
            Vector2 contact = new Vector2(contactX, contactY);

            collision = new CollisionInfo(a, b, normal, depth, contact);
            return true;
        }

        return false;
    }

    private void ResolveCollision(CollisionInfo collision)
    {
        var rbA = rigidbodyMapper[collision.EntityA];
        var rbB = rigidbodyMapper[collision.EntityB];
        var transformA = transformMapper[collision.EntityA];
        var transformB = transformMapper[collision.EntityB];

        // Skip if both are static
        if (rbA.Type == Rigidbody.BodyType.Static && rbB.Type == Rigidbody.BodyType.Static)
            return;

        if (collision.Depth < 0.01f) // 1mm threshold
            return;

        // Calculate relative velocity
        Vector2 relativeVelocity = rbB.Velocity - rbA.Velocity;
        float velocityAlongNormal = Vector2.Dot(relativeVelocity, collision.Normal);

        // Do not resolve if objects are separating
        if (velocityAlongNormal > 0) return;

        // Calculate restitution (bounciness)
        float e = (rbA.Restitution + rbB.Restitution) / 2;

        // Calculate impulse scalar
        float invMassA = rbA.Type == Rigidbody.BodyType.Dynamic ? 1.0f / rbA.Mass : 0;
        float invMassB = rbB.Type == Rigidbody.BodyType.Dynamic ? 1.0f / rbB.Mass : 0;

        float j = -(1 + e) * velocityAlongNormal;
        j /= (invMassA + invMassB);

        // Apply impulse
        Vector2 impulse = collision.Normal * j;


        // Positional correction to prevent sinking
        const float percent = 0.20f; // 20% correction
        const float slop = 0.01f; // 1cm slop

        Vector2 correction = collision.Normal * (Math.Max(collision.Depth - slop, 0) / (invMassA + invMassB)) * percent;

        if (rbA.Type == Rigidbody.BodyType.Dynamic)
        {

            // Only apply correction if not already sleeping
            if (rbA.IsAwake)
            {
                rbA.Velocity -= impulse * invMassA;
                transformA.Position -= correction * invMassA;
            }

            rbA.IsAwake = true;
        }

        if (rbB.Type == Rigidbody.BodyType.Dynamic)
        {

            // Only apply correction if not already sleeping
            if (rbB.IsAwake)
            {
                rbB.Velocity += impulse * invMassB;
                transformB.Position += correction * invMassB;
            }

            rbB.IsAwake = true;
        }
    }

    private Vector2[] GetRotatedRectangleCorners(Transform transform, Shape shape)
    {
        Vector2 halfSize = shape.Size / 2;
        Vector2[] corners = new Vector2[4];

        // Untransformed corners
        Vector2[] localCorners = new Vector2[]
        {
            new Vector2(-halfSize.X, -halfSize.Y),
            new Vector2(halfSize.X, -halfSize.Y),
            new Vector2(halfSize.X, halfSize.Y),
            new Vector2(-halfSize.X, halfSize.Y)
        };

        // Apply rotation and translation
        float cos = MathF.Cos(transform.Rotation.Z);
        float sin = MathF.Sin(transform.Rotation.Z);

        for (int i = 0; i < 4; i++)
        {
            Vector2 corner = localCorners[i];
            corners[i] = transform.Position + new Vector2(
                corner.X * cos - corner.Y * sin,
                corner.X * sin + corner.Y * cos
            );
        }

        return corners;
    }
    public override void Draw(nint renderer) { }
}