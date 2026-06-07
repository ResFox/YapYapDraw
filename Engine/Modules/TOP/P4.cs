using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class P4 : ISpecialAction
{
    public override string Name => "Wave Cannon";

    public override uint Phase => 4u;

    public override uint WeatherID => 89u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31616u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
