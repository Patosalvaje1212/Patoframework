


using System.Numerics;
using Raylib_cs;

namespace PatoframeWork.Rendering;

public class CameraManager
{
    public Camera2D cam;
    public float Velocity = 15f;
    public bool freeRoam;

    static CameraManager? instance;    
    public static CameraManager I
    {
        get
        {
            if(instance == null) instance = new();

            return instance;
        }
    }

    CameraManager()
    {
        cam.Offset = Vector2.Zero;
        cam.Rotation = 0f;
        cam.Zoom = 1f;
        cam.Target = Vector2.Zero;
    }

    public void UpdateCycle()
    {
        if(freeRoam)
        {
            if(Raylib.IsKeyDown(KeyboardKey.D))
            {
                cam.Offset += Vector2.UnitX * Velocity;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.A))
            {
                cam.Offset += Vector2.UnitX * -Velocity;
            }
            
            if(Raylib.IsKeyDown(KeyboardKey.W))
            {
                cam.Offset += Vector2.UnitY * -Velocity;
            }

            if(Raylib.IsKeyDown(KeyboardKey.S))
            {
                cam.Offset += Vector2.UnitY * Velocity;
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
                cam.Zoom += Velocity / 150;
            }

            if(Raylib.IsKeyDown(KeyboardKey.X))
            {
                cam.Zoom += -Velocity / 150;
            }
        }
    }

    public void ResetCamera()
    {
        cam.Offset = Vector2.Zero;
        cam.Rotation = 0f;
        cam.Zoom = 1f;
    }
}