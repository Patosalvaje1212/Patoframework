

using System.Collections;

namespace PF;

public abstract class BehaviourMap
{
    public abstract IDictionary GetMapper();
    public abstract bool Contains(Actor actor);

    public class Map<T> : BehaviourMap where T : class
    {
        private readonly Dictionary<Actor, T> map = new Dictionary<Actor, T>();

        public override bool Contains(Actor actor) => map.ContainsKey(actor);

        public override IDictionary GetMapper()
        {
            return map;
        }
    }
}

