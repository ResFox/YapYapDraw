using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DancingMad.P3;

public class CursedEdict : ISpecialAction
{
    public override string Name => "Cursed Edict";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47873u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
