using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CE105;

public class LethalNails : ISpecialAction
{
    public override string Name => "LethalNails";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41315u, 41316u, 41317u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
