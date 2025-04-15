
using System.Numerics;
using Raylib_cs;

namespace PatoFramework.Rendering;


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
    public Camera2D Cam;


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

    public Vector2 CamDelta => lastPos - Cam.Target;

    CameraManager()
    {
        ResetCamera();
    }


    /// <summary>
    /// Camera logic. Is called each frame by <c>GameController</c>
    /// </summary>
    public void UpdateCamera()
    {
        lastPos = Cam.Target;
        lastRotZoom = new Vector2(Cam.Rotation, Cam.Zoom);


        if(freeRoam)
        {
            if(Raylib.IsKeyDown(KeyboardKey.D))
            {
                Cam.Target += Vector2.UnitX * Velocity / Cam.Zoom;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.A))
            {
                Cam.Target += Vector2.UnitX * -Velocity / Cam.Zoom;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.W))
            {
                Cam.Target += Vector2.UnitY * -Velocity / Cam.Zoom;
            }

            if(Raylib.IsKeyDown(KeyboardKey.S))
            {
                Cam.Target += Vector2.UnitY * Velocity / Cam.Zoom;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.Q))
            {
                Cam.Rotation += Velocity / 15;
            }

            if(Raylib.IsKeyDown(KeyboardKey.E))
            {
                Cam.Rotation += -Velocity / 15;
            }

            if(Raylib.IsKeyDown(KeyboardKey.Z))
            {
                Cam.Zoom += Velocity / 1500;
            }

            if(Raylib.IsKeyDown(KeyboardKey.X))
            {
                Cam.Zoom += -Velocity / 1500;
            }
        }
    }


    public void MoveCam(Vector2 pos)
    {
        Cam.Target = pos;
    }

    /// <summary>
    /// Resets all of the Camera's values.
    /// </summary>
    public void ResetCamera()
    {
        Cam.Target = Vector2.Zero;
        Cam.Offset = GameController.WindowPixelSize / 2;
        Cam.Rotation = 0f;
        Cam.Zoom = 1f;
    }
}