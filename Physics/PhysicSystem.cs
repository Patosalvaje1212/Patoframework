using System.Numerics;

namespace PF.Physics;

public class PhysicsSystem : ActorSystem
{
    private Vector2 gravity;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    Dictionary<Actor, Rigidbody> rigidbodyMapper;
    Dictionary<Actor, Transform> transformMapper;

    public const int STEPS_SECOND = 60;
    private const double _stepTime = 1d / STEPS_SECOND;

    private double currTimeCounter = 0;

    public PhysicsSystem(World world, float m) : this(world)
    {
        gravity *= m;
    }
    public PhysicsSystem(World world) : this(world, new Vector2(0, 9.81f) * 100)
    {
    }
    public PhysicsSystem(World world, Vector2 gravity) : base(world)
    {
        this.gravity = gravity;
        AddRequiredType(typeof(Rigidbody), typeof(Transform));
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void Init()
    {
        rigidbodyMapper = world.GetMapper<Rigidbody>();
        transformMapper = world.GetMapper<Transform>();
    }

    public override void Update(double deltaTime)
    {
        currTimeCounter += deltaTime;

        while (currTimeCounter >= _stepTime)
        {
            currTimeCounter -= _stepTime;
            PhysicsStep(_stepTime);
        }
    }

    void PhysicsStep(double deltaTime)
    {
        // Apply physics to all entities with Rigidbody and Transform
        foreach (var actor in mActors)
        {
            // If entity is innactive, continue
            if(!actor) continue;


            var rigidbody = rigidbodyMapper[actor];
            var transform = transformMapper[actor];

            // Skip static bodies
            if (rigidbody.Type == Rigidbody.BodyType.Static) continue;

            // Check if body should sleep
            if (rigidbody.Velocity.LengthSquared() < 0.01f)
            {
                rigidbody.IsAwake = false;
                rigidbody.Velocity = Vector2.Zero;
                rigidbody.Force = Vector2.Zero;
                continue; // Skip physics for sleeping bodies
            }

            // Apply gravity to dynamic bodies
            if (rigidbody.Type == Rigidbody.BodyType.Dynamic)
            {
                rigidbody.Force += gravity * rigidbody.Mass * rigidbody.GravMult;
            }

            // Integrate forces
            if (rigidbody.Mass > 0)
            {
                // Calculate acceleration (F = ma -> a = F/m)
                Vector2 acceleration = rigidbody.Force / rigidbody.Mass;

                // Update velocity (v = u + at)
                rigidbody.Velocity += acceleration * (float)deltaTime;

                // Apply damping (air resistance)
                if (rigidbody.IsAwake)
                {
                    rigidbody.Velocity *= 0.99f;
                }

                // Update position (s = vt)
                transform.Position += rigidbody.Velocity * (float)deltaTime;

                // Reset forces
                rigidbody.Force = Vector2.Zero;

                //SDL3.SDL.LogInfo(SDL3.SDL.LogCategory.Application, ((float)deltaTime).ToString());

            }
        }
    }

    public void SetGravity(Vector2 newGravity)
    {
        gravity = newGravity;
    }

    public override void Draw(nint renderer)
    {
        //
    }
}