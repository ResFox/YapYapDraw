using System.Collections.Generic;
using YapYapDraw.Engine.Module;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ShishuDeep;

public class FairyPellyFireRing : ISpecialAction
{
    public override string Name => "Fairy Pelly Fire Ring";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45447u };

    public override void OnActionCast(ActorCastInfo info)
    {
        AutoDrawModule.Run(info);
    }
}
