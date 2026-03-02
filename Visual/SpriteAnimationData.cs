using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PF.Visual;
public struct SpriteAnimationData
{
    public List<(int, float)[]> animations;

    /// <summary>
    /// Initializes a new <c>SpriteAnimationData</c> and assigns its data.
    /// </summary>
    /// <param name="values">
    /// Arrays of tuples containing:
    /// <list type="bullet">
    /// <item>
    /// <b>Item1 - int</b>: index of the texture to use.
    /// </item>
    /// <item>
    /// <b>Item2 - float</b>: time in seconds to mantain this texture fror.
    /// </item>
    /// </list>
    /// </param>
    public SpriteAnimationData(params (int, float)[][] values)
    {
        animations = [.. values];
    }

    public readonly void AddAnimation((int, float)[] value) => animations.Add(value);

    public readonly float GetAnimTime(int animNumber, int frameNumber) => animations[animNumber][frameNumber].Item2;
    public readonly int GetAnimFrameIndex(int animNumber, int frameNumber) => animations[animNumber][frameNumber].Item1;

    public readonly int GetMaxFrames(int animNumber) => animations[animNumber].Length;
}
