using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PF.Misc.Signal;

public class Signal
{

    public bool sendSignal {get; private set;} = false;
    public bool signalValue  {get; private set;} = false;

    public enum SignalType
    {
        Basic,
        Create,
        Delete
    } 

    public List<KeyValuePair<Actor, SignalType>> signals {get; private set;} = []; 

    public Action<bool, Actor> receiveSignal = (b, res) => {};

    

    public void SendSingal(bool value)
    {
        sendSignal = true;
        signalValue = false;
    }


    /// <summary>
    ///  Resets signal state, and cancels the emission. This method is intended for internal use, do not call it unless you want to cancel the signals before they reach their target.
    /// </summary>
    public void SignalSent()
    {
        sendSignal = false;
    }
}
