using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TheMesoTerminal;

public class Keraunography : ISpecialAction
{
    public override string Name => "Keraunography";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43813u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info.Pos, 60f, 10f, 0f, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 45176u }
        });
    }
}
