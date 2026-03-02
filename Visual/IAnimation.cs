using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PF.Visual;

public interface IAnimation
{
    public void ChangeAnim(int newAnim, bool resetFrame = true);
    public void ChangeFrame(int newFrame);
    void UpdateRender();
}
