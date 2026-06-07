using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M10S;

public class BlazeImpact : ISpecialAction
{
    public override string Name => "Blaze Impact";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46518u, 46464u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId.GameObject(), 6f, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
