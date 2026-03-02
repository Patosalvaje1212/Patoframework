

using System.Numerics;
namespace PF.Misc;


public class TweenSystem : ActorSystem
{



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static TweenSystem I;


    HashSet<Tween> tweens = [];
    HashSet<Tween> queue = [];
    HashSet<Tween> purge = [];

    private bool updating = false;
    public TweenSystem(World world) : base(world)
    {
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void Init()
    {
        I = this;
    }

    public override void Draw(nint renderer)
    {
    }



    public async Task AddTweenAndWaitForCompletion(Action<float> setter, float from, float to, float time, TweenType easeType = TweenType.Linear)
    {
        AddTween(new Tween(setter, from, to, time, easeType));

        await Task.Delay((int)time * 1000);
    }

    public void AddTween(Action<float> setter, float from, float to, float time, TweenType easeType = TweenType.Linear)
    {
        AddTween(new Tween(setter, from, to, time, easeType));
    }


    public void AddTween(Func<Vector2> getter, Action<Vector2> setter, Vector2 from, Vector2 to, float time, TweenType easeType = TweenType.Linear)
    {
        AddTween(new Tween((res) => {Vector2 d = getter(); setter(new Vector2(d.X, res));}, from.X, to.X, time, easeType));
        AddTween(new Tween((res) => {Vector2 d = getter(); setter(new Vector2(res, d.Y));}, from.Y, to.Y, time, easeType));
    }

    async void AddTween(Tween action)
    {
        while(updating)
            await Task.Delay(1);

        tweens.Add(action);
    }

    public override void Update(double deltaTime)
    {
        updating = true;
        foreach (var tween in new HashSet<Tween>(tweens))
        {
            if(tween.Finished)
            {
                purge.Add(tween);
            } 
            else
            {
                tween.Update(deltaTime);
            }
        }
        
        foreach (var tween in purge)
        {
            tweens.Remove(tween);
        }

        purge.Clear();
        
        updating = false;
    }
}

public enum TweenType
{
    Linear,
    InSine,
    OutSine,
    InOutSine,

    InCubic,
    OutCubic,
    InOutCubic,

    InBack,
    OutBack,
    InOutBack,

    InBounce,
    OutBounce,
    InOutBounce,

    InElastic,
    OutElastic,
    InOutElastic,
}

internal class Tween(Action<float> setter, float from, float to, float time, TweenType easeType)
{
    public readonly float time = time;
    
    public float cTime = 0;
    
    public readonly float from = from;
    public readonly float to = to;
    public readonly Action<float> setter = setter;

    public readonly TweenType easeType = easeType;

    public bool Finished { get; private set; }

    public void Update(double delta)
    {
        cTime += (float)delta;

        float m = GetEase(cTime / time, easeType);
        float cVal = (to - from) * m + from;

        if(cTime > time)
        {
            cVal = to;
            Finished = true;
        }

        setter(cVal);
    }

    public override bool Equals(object? obj)
    {
        if(obj is Tween other)
        {
            if(setter == other.setter) return true;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return setter?.GetHashCode() ?? 0;
    }


    float GetEase(float r, TweenType easeType)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;
        float c3 = c1 + 1f;
        float c4 = (2 * MathF.PI) / 3f;
        float c5 = (2f * MathF.PI) / 4.5f;

        float n1 = 7.5625f;
        float d1 = 2.75f;

        float res = 0f;

        switch (easeType)
        {
            case TweenType.Linear :
            default :
                res = r;
            break;
            case TweenType.InSine: 
                res =  1f - MathF.Cos((r * MathF.PI) / 2f);
            break;

            case TweenType.OutSine:
                res = MathF.Sin((r * MathF.PI) / 2f);
            break;

            case TweenType.InOutSine :
                res = -(MathF.Cos(MathF.PI * r) - 1f) / 2f;
            break;

            case TweenType.InCubic :
                res = r * r * r;
            break;
                
            case TweenType.OutCubic :
                res = 1f - MathF.Pow(1f - r, 3f);
            break;
            
            case TweenType.InOutCubic :
                res = r < 0.5 ? 4 * r * r * r : 1 - MathF.Pow(-2f * r + 2f, 3f) / 2f;
            break;
            
            case TweenType.InBack :
                res = c3 * r * r * r - c1 * r * r;
            break;
            
            case TweenType.OutBack :
                res = 1 + c3 * MathF.Pow(r - 1f, 3f) + c1 * MathF.Pow(r - 1f, 2f);
            break;
            
            case TweenType.InOutBack :
                res = r < 0.5
                    ? MathF.Pow(2 * r, 2) * ((c2 + 1) * 2 * r - c2) / 2f
                    : (MathF.Pow(2 * r - 2, 2) * ((c2 + 1) * (r * 2 - 2) + c2) + 2) / 2f;
            break;
            
            case TweenType.InBounce :
                res = 1 - GetEase(1 - r, TweenType.OutBounce);
            break;
            
            case TweenType.OutBounce :
                if (r < 1 / d1) {
                    res = n1 * r * r;
                } else if (r < 2 / d1) {
                    res = n1 * (r -= 1.5f / d1) * r + 0.75f;
                } else if (r < 2.5 / d1) {
                    res = n1 * (r -= 2.25f / d1) * r + 0.9375f;
                } else {
                    res = n1 * (r -= 2.625f / d1) * r + 0.984375f;
                }
            break;
            
            case TweenType.InOutBounce :
                res = r < 0.5
                    ? (1 - GetEase(1 - 2 * r, TweenType.OutBounce)) / 2f
                    : (1 + GetEase(2 * r - 1, TweenType.OutBounce)) / 2f;
            break;
            
            case TweenType.InElastic :
                res = r == 0
                    ? 0
                    : r == 1
                    ? 1
                    : -MathF.Pow(2, 10 * r - 10) * MathF.Sin((r * 10 - 10.75f) * c4);
            break;
            
            case TweenType.OutElastic :
                res = r == 0
                    ? 0
                    : r == 1
                    ? 1
                    : MathF.Pow(2, -10 * r) * MathF.Sin((r * 10 - 0.75f) * c4) + 1;
            break;
            
            case TweenType.InOutElastic :
                if (r == 0f) res = 0f;
                else
                if (r == 1f) res = 1f;
                else
                if (r < 0.5f)
                {
                    res = -0.5f * MathF.Pow(2f, 20f * r - 10f) * 
                        MathF.Sin((20f * r - 11.125f) * c5);
                }
                else
                {
                    res = 0.5f * MathF.Pow(2f, -20f * r + 10f) * 
                        MathF.Sin((20f * r - 11.125f) * c5) + 1f;
                }
            break;

        }

        return res;
    }
}