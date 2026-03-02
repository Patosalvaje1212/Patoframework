
namespace PF;

public class Actor
{
    readonly Guid guid;
    public bool active = true;

    public Actor()
    {
        guid = Guid.NewGuid();
    }

    private Actor(Actor id)
    {
        guid = id;
    }

    public static implicit operator Guid(Actor g) => g.guid;
    public static implicit operator bool(Actor g) => g.active;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is Actor other) return guid.Equals(other.guid);
        if (obj is Actor g) return guid.Equals(g);
        return false;
    }

    public override int GetHashCode()
    {
        return guid.GetHashCode();
    }
}