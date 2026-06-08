using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class GreatWhirlwind : ISpecialAction
{
    public override string Name => "Great Whirlwind";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11073u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 8f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 11073u }
        });
    }
}
