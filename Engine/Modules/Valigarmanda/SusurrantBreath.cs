using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Valigarmanda;

public class SusurrantBreath : ISpecialAction
{
    public override string Name => "Susurrant Breath";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36808u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(new Vector3(100f, 0f, 75f), 50f, 80, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 36808u }
        });
    }
}
