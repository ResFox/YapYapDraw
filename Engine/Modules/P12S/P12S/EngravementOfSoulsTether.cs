using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P12S.P12S;

public class EngravementOfSoulsTether : ISpecialAction
{
    public override string Name => "Engravement of Souls (tether)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33521u, 33522u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.RectangleToTarget(info, 60f, 3f);
    }
}
