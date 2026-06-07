using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class NeoBombarianSpecial : ISpecialAction
{
    public override string Name => "Neo Bombarian Special";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42364u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.KnockBack(info, 60f);
    }
}
