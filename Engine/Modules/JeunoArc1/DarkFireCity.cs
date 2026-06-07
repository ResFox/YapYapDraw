using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class DarkFireCity : ISpecialAction
{
    public override string Name => "Dark Fire (city)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40782u };

    public override uint Phase => 4u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 11.5f, 11.5f, 11.5f);
    }
}
