using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PF.Visual;

public interface IRender
{
    /// <summary>
    /// Called each time for handling the Rendering logic
    /// </summary>
    /// <param name="delta">Time passed since last call</param>
    public void RenderAt(Transform transform);
    
    /// <summary>
    /// Called each frame with the time passed since last frame. Intended for animations. 
    /// </summary>
    /// <param name="delta">Time passed since last call</param>
    public void AdvanceTime(double delta);
}
