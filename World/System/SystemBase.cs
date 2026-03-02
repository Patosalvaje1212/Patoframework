
namespace PF;


/// <summary>
/// Base interface for all Systems in the ECS.
/// </summary>
public interface SystemBase
{
    /// <summary>
    /// Called once, before any rendering occurs.
    /// </summary>
    public void Init();

    /// <summary>
    /// Called each frame before <c>Render</c>.
    /// </summary>
    /// <param name="deltaTime">Time passed since last call</param>
    public void Update(double deltaTime);

    /// <summary>
    /// Called each frame after <c>Update</c>
    /// </summary>
    /// <param name="renderer">Time passed since last call</param>
    public void Draw(nint renderer);
}