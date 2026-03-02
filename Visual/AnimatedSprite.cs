using Raylib_cs;

namespace PF.Visual;

public class AnimatedSprite : Sprite
{
    Texture2D[] Textures;
    SpriteAnimationData AnimationData;

    double counter = 0;
    int currentAnim = 0;
    int currentFrame = 0;

    public AnimatedSprite(Texture2D[] textures, SpriteAnimationData animationData) : base(textures[0])
    {
        Textures = textures;
        AnimationData = animationData;
    }

    public override void AdvanceTime(double delta)
    {
        base.AdvanceTime(delta);

        counter += delta;

        if(counter > AnimationData.GetAnimTime(currentAnim, currentFrame))
        {
            counter -= AnimationData.GetAnimTime(currentAnim, currentFrame);

            currentFrame ++;

            if(AnimationData.GetMaxFrames(currentAnim) == currentFrame)
            {
                currentFrame = 0;
            }

            UpdateTexture();
        }
    }

    public void ChangeAnim(int newAnim, bool resetFrame = true)
    {
        currentAnim = newAnim;

        if(resetFrame)
            currentFrame = 0;

        UpdateTexture();
    }

    public void ChangeFrame(int newFrame)
    {
        currentFrame = newFrame;

        UpdateTexture();
    }

    void UpdateTexture() => Texture = Textures[AnimationData.GetAnimFrameIndex(currentAnim, currentFrame)];
}
