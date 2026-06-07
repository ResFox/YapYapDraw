using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7;

public class NeoBombarianSpecial : ISpecialAction
{
    public override string Name => "NeoBombarianSpecial";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42287u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.KnockBack(info, 90f, 1000f);
    }
}
