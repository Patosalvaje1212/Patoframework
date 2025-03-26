
using System.Numerics;
using Raylib_cs;

namespace PatoframeWork.Rendering;


/// <summary>
/// Class to handle the camera logic
/// </summary>
public class CameraManager
{
    static CameraManager? instance;

    public static CameraManager I
    {
        get
        {
            instance ??= new();

            return instance;
        }
    }

    /// <summary>
    /// Target 2D Camera.
    /// </summary>
    public Camera2D cam;

    /// <summary>
    /// Camera move Velocity.
    /// </summary>
    public float Velocity = 12f;

    /// <summary>
    /// If <c>true</c>, in Debug mode, move the camera with WASD, rotate it with QE, and change the zoom value with ZX. 
    /// </summary>
    public bool freeRoam;

    
    /// <summary>
    /// Stores the Camera's last position.
    /// </summary>
    public Vector2 lastPos;
    /// <summary>
    /// Stores the Camera's last Rotation and Zoom values.
    /// </summary>
    public Vector2 lastRotZoom;

    

    CameraManager()
    {
        ResetCamera();
    }


    /// <summary>
    /// Camera logic. Is called each frame by <c>GameController</c>
    /// </summary>
    public void UpdateCamera()
    {
        lastPos = cam.Target;
        lastRotZoom = new Vector2(cam.Rotation, cam.Zoom);


        if(freeRoam)
        {
            if(Raylib.IsKeyDown(KeyboardKey.D))
            {
                cam.Target += Vector2.UnitX * Velocity / cam.Zoom;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.A))
            {
                cam.Target += Vector2.UnitX * -Velocity / cam.Zoom;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.W))
            {
                cam.Target += Vector2.UnitY * -Velocity / cam.Zoom;
            }

            if(Raylib.IsKeyDown(KeyboardKey.S))
            {
                cam.Target += Vector2.UnitY * Velocity / cam.Zoom;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.Q))
            {
                cam.Rotation += Velocity / 15;
            }

            if(Raylib.IsKeyDown(KeyboardKey.E))
            {
                cam.Rotation += -Velocity / 15;
            }

            if(Raylib.IsKeyDown(KeyboardKey.Z))
            {
                cam.Zoom += Velocity / 1500;
            }

            if(Raylib.IsKeyDown(KeyboardKey.X))
            {
                cam.Zoom += -Velocity / 1500;
            }
        }
    }

    /// <summary>
    /// Resets all of the Camera's values.
    /// </summary>
    public void ResetCamera()
    {
        cam.Target = Vector2.Zero;
        cam.Offset = new Vector2( Raylib.GetScreenWidth(), Raylib.GetScreenHeight() )/ 2;
        cam.Rotation = 0f;
        cam.Zoom = 1f;
    }
}