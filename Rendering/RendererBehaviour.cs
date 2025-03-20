using System.Text.Json.Serialization;
using Raylib_cs;

namespace PatoframeWork.Rendering;




public class RendererBehaviour : Behaviour
{
    public Color Color = Color.Blue;
    public int Size = 15;

    public string texturePath = "";

    
    public float zRot;
    public int Order;

    public override void OnAdd()
    {
        GameController.I.Renderers.Add(this);
    }

    public override void OnRemove()
    {
        GameController.I.Renderers.Remove(this);
    }

    public override void UpdateEffect()
    {
        //
    }
}