using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CE106;

public class CrystallizedEnergy : ISpecialAction
{
    public override string Name => "CrystallizedEnergy";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42732u, 41758u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
