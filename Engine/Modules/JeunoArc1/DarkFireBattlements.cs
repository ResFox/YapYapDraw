using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.JeunoArc1;

public class DarkFireBattlements : ISpecialAction
{
    public override string Name => "Dark Fire (battlements)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40783u };

    public override uint Phase => 4u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.HotWing(new Vector3(150f, -500f, 800f), 20f, 3f, 14.5f, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 40783u }
        });
        SimpleElement.HotWing(new Vector3(150f, -500f, 800f), 20f, 3f, 14.5f, info.Facing + 90.Degrees(), 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 40783u }
        });
    }
}
