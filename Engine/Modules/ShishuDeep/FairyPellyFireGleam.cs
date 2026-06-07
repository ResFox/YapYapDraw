using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ShishuDeep;

public class FairyPellyFireGleam : ISpecialAction
{
    public override string Name => "Fairy Pelly Fire Gleam";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45499u, 47397u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Cross(info);
    }
}
