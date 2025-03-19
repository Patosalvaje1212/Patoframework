

using System.Security.Cryptography.X509Certificates;
using Raylib_cs;

namespace PatoframeWork.Tweening;

public class Tween<T>(T startVal, T endVal, float duration, Action<T> onUpdateTween)
{
    public required object Target;
    public bool isPaused;
    public ulong ID;


    private T _startVal = startVal;
    private T _endVal = endVal;
    private float _duration = duration;
    private float _runTime = 0f;
    private Action<T> _onUpdateTween = onUpdateTween;

    void UpdateTween()
    {
        
    }

    /*public T Interpolation(T start, T end, float t)
    {
        Raymath.Lerp

    }*/


    void KillTween()
    {

    }
    void TargetDestroyed()
    {

    }

    void Pause()
    {

    }
    void Resume()
    {

    }
}