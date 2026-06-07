using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Valigarmanda;

public class CracklingCataclysm : ISpecialAction
{
    public override string Name => "Crackling Cataclysm";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36801u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 6f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 36801u }
        });
    }
}
