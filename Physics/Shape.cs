using System.Numerics;

namespace PF.Physics;

public class Shape
{
    public ShapeType Type;
    public Vector2 Size; // For rectangle: width/height, for circle: radius in both

    public Shape(ShapeType type, float radius)
    {
        Type = type;
        Size = new Vector2(radius, radius);
    }

    public Shape(ShapeType type, float width, float height)
    {
        Type = type;
        Size = new Vector2(width, height);
    }

    public enum ShapeType
    {
        Circle,
        Rectangle
    }
}

