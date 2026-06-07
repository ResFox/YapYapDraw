using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.UnendingCoil;

public class LightningMarker : ISpecialAction
{
    public override string Name => "Lightning (marker)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 466)
        {
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "m0420tar_5m0f"
            }, info.TargetID.GameObject());
        }
    }
}
