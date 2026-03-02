using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PF.Physics;

public class Transform
{
    private Raylib_cs.Transform t;

    public Transform(Vector2 pos, Quaternion? q = null, Vector2? scale = null)
    {
        t = new()
        {
            Translation = new(pos, 0f),
            Rotation = q ?? Quaternion.Identity,
            Scale = new(scale ?? Vector2.One, 1f)
        };
    }

    private Transform(Raylib_cs.Transform t)
    {
        this.t = t;
    }


    public static implicit operator Raylib_cs.Transform(Transform res)
    {
        return res.t;
    }

    public static implicit operator Transform(Raylib_cs.Transform res)
    {
        return new(res);
    }

    public Raylib_cs.Transform Get()
    {
        return t;
    }

    public Vector2 Position
    {
        get
        {
            return t.Translation.AsVector2();
        }
        set
        {
            t.Translation = value.AsVector3();
        }
    }
    public Quaternion Rotation
    {
        get
        {
            return t.Rotation;
        }
        set
        {
            t.Rotation = value;
        }
    }
    public Vector2 Scale
    {
        get
        {
            return t.Scale.AsVector2();
        }
        set
        {
            t.Scale = value.AsVector3();
        }
    }

    public void TranslateLocal(Vector2 amount)
    {
        t.TranslateLocal(amount.AsVector3());
    }

    public void TranslateGlobal(Vector2 amount)
    {
        t.TranslateGlobal(amount.AsVector3());
    }
}
