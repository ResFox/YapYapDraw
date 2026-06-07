using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CloudOfDarkness;

public class GhastlyGloom : ISpecialAction
{
    public override string Name => "Ghastly Gloom (donut)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40460u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_sircle_4021x",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 40f,
            radiusZ = 40f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        });
    }
}
