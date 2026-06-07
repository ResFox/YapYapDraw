using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.StrayboroughEw;

public class ManusyaThunderIII : ISpecialAction
{
    public override string Name => "Manusya Thunder III";

    public override HashSet<uint> ActionID => new HashSet<uint> { 25239u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 3f, 4000f);
    }
}
