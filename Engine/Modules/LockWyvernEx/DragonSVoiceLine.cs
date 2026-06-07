using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class DragonSVoiceLine : ISpecialAction
{
    public override string Name => "Dragon's Voice (line)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43940u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineRect(info, 8f, 2500f, 5);
    }
}
