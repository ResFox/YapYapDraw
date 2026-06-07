using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.UWU;

public class Landslide : ISpecialAction
{
    public override string Name => "Earthquake";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11119u, 11120u, 11121u, 11298u, 11134u, 11135u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.SourceId.GameObject().BaseId == 8727)
        {
            SimpleElement.Rectangle(info, 44.55f, 3f, 4.55f);
        }
        else
        {
            SimpleElement.Rectangle(info, 40.5f, 3f, 0.5f);
        }
    }
}
