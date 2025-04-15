

using System.Numerics;
using PatoFramework;
using Raylib_cs;


namespace PatoFramework.Rendering;

public class LightBehaviour : Behaviour
{
    public bool IsDirty;

    public Color LightColor = Color.White;

    public float Zpos = 0.05f;


    float lastZpos;
    Color lastColor;
    Vector2 lastPos;


    public override void OnAdd()
    {
        LightsManager.AddLight(this);
        
        IsDirty = true;

        base.OnAdd();
    }

    public override void OnRemove()
    {
        LightsManager.RemoveLight(this);

        base.OnRemove();
    }

    public override void UpdateEffect()
    {
        base.UpdateEffect();

        if(lastPos != Owner.GlobalPosition) IsDirty = true;
        if(Raylib.ColorNormalize(lastColor) != Raylib.ColorNormalize(LightColor)) IsDirty = true;
        if(lastZpos != Zpos) IsDirty = true;

        lastPos = Owner.GlobalPosition;
        lastZpos = Zpos;
        lastColor = LightColor;
    }
}