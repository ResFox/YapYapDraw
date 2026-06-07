using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M11S;

public class SkyfallShatter : ISpecialAction
{
    public override string Name => "Skyfall Shatter";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46155u, 46157u, 46159u, 46161u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
