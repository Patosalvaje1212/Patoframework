


using Raylib_cs;

namespace PF.Visual;

public class AnimatedTileSprite : TileSprite
{

    Rectangle[] Rectangles;
    SpriteAnimationData AnimationData;


    double counter = 0;
    int currentAnim = 0;
    int currentFrame = 0;

    public AnimatedTileSprite(Texture2D texture, Rectangle[] rectangles, SpriteAnimationData animationData) : base(texture, rectangles[0])
    {
        Rectangles = rectangles;
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

            UpdateRender();
        }
    }

    public void ChangeAnim(int newAnim, bool resetFrame = true)
    {
        currentAnim = newAnim;

        if(resetFrame)
            currentFrame = 0;

        UpdateRender();
    }

    public void ChangeFrame(int newFrame)
    {
        currentFrame = newFrame;

        UpdateRender();
    }

    void UpdateRender() => sRect = Rectangles[AnimationData.GetAnimFrameIndex(currentAnim, currentFrame)];
}
