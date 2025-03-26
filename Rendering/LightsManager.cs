

using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

namespace PatoframeWork.Rendering;

public static class LightsManager
{
    
    private static readonly HashSet<LightBehaviour> Lights = [];


    public static int LightResolution { get; set; } = 5;
    public static Color AmbientLight { get; set; } = Raylib.ColorAlpha(Color.White, 1f);
    public static float LightFallOff { get; set; } = 0.5f;


    public static void AddLight(LightBehaviour light) => Lights.Add(light);
    public static void RemoveLight(LightBehaviour light) => Lights.Remove(light);


    static LightBehaviour[] lastNearLights = [];

    public static bool IsDirty()
    {
        if(CameraManager.I.lastPos != CameraManager.I.cam.Target
        || CameraManager.I.lastRotZoom != new System.Numerics.Vector2(CameraManager.I.cam.Rotation, CameraManager.I.cam.Zoom)) 
        return true;


        foreach (var light in Lights)
        {
            if(light.IsDirty) return true;
        }

        return false;
    }


    public static Vector3[] GetNearestLights(int amount = 5)
    {
        ResetDirty();

        LightBehaviour[] nearLights = new LightBehaviour[amount];

        List<LightBehaviour> orderedLights = [.. Lights.Where(res => res.Owner.Active).OrderBy(res => Raymath.Vector2Distance(CameraManager.I.cam.Target, res.Owner.GlobalPosition))];

        nearLights = [.. orderedLights.Where(res => orderedLights.IndexOf(res) < amount ) ];

        lastNearLights = nearLights;

        Vector3[] Pos = new Vector3[nearLights.Length];
        for (int i = 0; i < nearLights.Length; i++)
        {
            //Pos[i] = new Vector3(.5f, .5f, .5f);
            Pos[i] = WorldToScreenSpace( new Vector3(nearLights[i].Owner.GlobalPosition.X, nearLights[i].Owner.GlobalPosition.Y, nearLights[i].Zpos));
        }

        return Pos;
    }


    public static Vector4[] GetNearestLightsColors()
    {
        Vector4[] Colors = new Vector4[lastNearLights.Length];

        for (int i = 0; i < lastNearLights.Length; i++)
        {
            //Pos[i] = new Vector3(.5f, .5f, .5f);
            Colors[i] = Raylib.ColorNormalize(lastNearLights[i].LightColor);
        }

        return Colors;
    }

    static void ResetDirty()
    {
        foreach (var light in Lights)
        {
            light.IsDirty = false;
        }
    }


    static Vector3 WorldToScreenSpace(Vector3 position)
    {
        var screen = Raylib.GetWorldToScreen2D(new Vector2(position.X, position.Y), CameraManager.I.cam);
        
        return new Vector3(screen.X / Raylib.GetScreenWidth(), - screen.Y / Raylib.GetScreenHeight(), position.Z);
    }

    static Vector3 WorldToScreenSpace(Vector2 position)
    {
        return WorldToScreenSpace(new Vector3( position.X, position.Y, 0.05f));
    }
}