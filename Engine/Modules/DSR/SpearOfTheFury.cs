using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class SpearOfTheFury : ISpecialAction
{
    public override string Name => "Spear of the Fury";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27539u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 50f, 5f);
    }
}
