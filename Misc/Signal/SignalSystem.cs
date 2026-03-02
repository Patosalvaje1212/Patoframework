using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PF.Misc.Signal;

public class SignalSystem : ActorSystem
{

    public Dictionary<Actor, Signal> signalMapper;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public SignalSystem(World world) : base(world)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        AddRequiredType(typeof(Signal));

    }

    public override void Draw(nint renderer) { }

    public override void Init()
    {
        signalMapper = world.GetMapper<Signal>();

        foreach (var actor in mActors)
        {
            foreach (var signal in signalMapper[actor].signals)
            {
                if(signal.Value == Signal.SignalType.Create)
                    signal.Key.active = false;
            }
        }
    }

    public override void Update(double deltaTime)
    {
        foreach (var actor in mActors)
        {
            if(!actor) continue;

            Signal signal = signalMapper[actor];
            
            if(signal.sendSignal)
            foreach (var sg in signal.signals)
            {
                switch (sg.Value)
                {
                    case Signal.SignalType.Basic:
                        signalMapper[sg.Key].receiveSignal(signal.signalValue ,actor);
                    break;
                    case Signal.SignalType.Create:
                        sg.Key.active = true;
                    break;
                    case Signal.SignalType.Delete:
                        sg.Key.active = false;
                    break;
                }
            }
        }
    }
}