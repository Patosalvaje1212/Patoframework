using System.Text.Json.Serialization;
using Raylib_cs;

namespace PatoframeWork.Rendering;




public class RendererBehaviour : Behaviour
{
    public Color Color = Color.White;
    public float Size = 15;

<<<<<<< Updated upstream
=======
    public enum ShapeType
    {
        Circle, Square, Image
    }

    public ShapeType RenderType;

    
    public ulong ImageID, SpriteID;

    
    public float zRot;
>>>>>>> Stashed changes
    public int Order;

    public override void OnAdd()
    {
        GameController.I.renderers.Add(this);
    }

    public override void OnRemove()
    {
        GameController.I.renderers.Remove(this);
    }

    public override void UpdateEffect()
    {
        //
    }
}