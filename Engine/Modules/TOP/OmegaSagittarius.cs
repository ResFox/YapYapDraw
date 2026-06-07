using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class OmegaSagittarius : ISpecialAction
{
    public override string Name => "Omega Sagittarius";

    public override uint Phase => 2u;

    public override uint WeatherID => 78u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31539u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
