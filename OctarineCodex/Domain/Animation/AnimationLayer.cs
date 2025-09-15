using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Individual animation layer within a layered animation system.
/// </summary>
public class AnimationLayer
{
    private readonly SimpleAnimationComponent _animation = new();

    public AnimationLayer(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }

    public string Name { get; }
    public int Priority { get; }
    public bool IsVisible { get; set; } = true;
    public float Alpha { get; set; } = 1.0f;
    public bool IsComplete => _animation.IsComplete;

    public void SetAnimation(LDtkAnimationData animationData)
    {
        _animation.SetAnimation(animationData);
    }

    public void Update(GameTime gameTime)
    {
        _animation.Update(gameTime);
    }

    public void Stop()
    {
        _animation.StopAnimation();
    }

    public int GetCurrentFrame()
    {
        return _animation.GetCurrentFrame();
    }

    public int GetCurrentTileId()
    {
        return _animation.GetCurrentTileId();
    }
}
