using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P3N;

public class BirdOfPrey : ISpecialAction
{
    public override string Name => "Bird of Prey";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30723u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
