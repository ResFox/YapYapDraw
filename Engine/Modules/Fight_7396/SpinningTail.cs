using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_7396;

public class SpinningTail : ISpecialAction
{
    public override string Name => "Spinning Tail";

    public override HashSet<uint> ActionID => new HashSet<uint> { 10348u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        SimpleElement.Fan(info.Source.Position, 11f, 180, info.Source.Rotation.Radians() - 90.Degrees(), 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 11451u }
        });
    }
}
