
using System.Numerics;
using Raylib_cs;

namespace PatoframeWork.Rendering;

public class CameraManager
{
    public Camera2D cam;
    public float Velocity = 12f;
    public bool freeRoam;

    static CameraManager? instance;  
    
    public Vector2 lastPos;
    public Vector2 lastRotZoom;

    public static CameraManager I
    {
        get
        {
            instance ??= new();

            return instance;
        }
    }

    CameraManager()
    {
        ResetCamera();
    }

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

    public void ResetCamera()
    {
        cam.Target = Vector2.Zero;
        cam.Offset = new Vector2( Raylib.GetScreenWidth(), Raylib.GetScreenHeight() )/ 2;
        cam.Rotation = 0f;
        cam.Zoom = 1f;
    }
}