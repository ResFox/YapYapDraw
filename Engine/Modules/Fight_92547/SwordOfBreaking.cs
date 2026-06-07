using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_92547;

public class SwordOfBreaking : ISpecialAction
{
    public override string Name => "Sword of Breaking";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43112u, 43113u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info.Pos, 24f, 32f, 0f, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
