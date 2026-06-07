using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M1S;

public class SplinteringNails : ISpecialAction
{
    private IGameObject? iconTarget;

    public override string Name => "Splintering Nails";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38041u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (iconTarget != Svc.Objects.LocalPlayer)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan045_1bf",
                radiusX = 100f,
                radiusZ = 100f,
                target = (IGameObject?)Svc.Objects.LocalPlayer,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38042u }
                }
            }, info.SourceId.GameObject());
        }
    }

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 538)
        {
            iconTarget = target;
        }
    }
}
