using PatoframeWork;
using Raylib_cs;
using System.Numerics;


namespace PatoframeWork.Physics;

public class PhysicBehaviour : Behaviour
{

    // Collision. Applied each frame to its Owner(or trying it :).

    
    //Create a world.
    
 
    private Body _playerBody;
    private float _playerBodyRadius = 1.5f / 2f; // player diameter is 1.5 meters .
    // Create the player
    Vector2 playerPosition = new Vector2(0, _playerBodyRadius);
    _playerBody = _world.CreateBody(playerPosition, 0, BodyType.Dynamic);
    Fixture pfixture = _playerBody.CreateCircle(_playerBodyRadius, 1f);
    // Give it some bounce and friction.
    //pfixture.Restitution = 0.3f;
    //pfixture.Friction = 0.5f;



    // Moving velocity. Applied each frame to its Owner.

    public Vector2 Velocity = Vector2.Zero;


    // Update Funtion.

    public override void UpdateEffect()
    {
        Owner.GlobalPosition += Velocity;
    }
}