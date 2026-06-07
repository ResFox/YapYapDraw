using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_56400;

public class ArmorDeployment : ISpecialAction
{
    public override string Name => "Armor Deployment";

    public override HashSet<uint> ActionID => new HashSet<uint> { 24474u, 24475u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 24474)
        {
            uint sourceId = info.SourceId;
            Angle rotation = info.Facing - 90.Degrees();
            SimpleElement.Rectangle(sourceId, 42f, 22f, 0f, null, rotation, 3000f, 0f, 24538u);
        }
        else
        {
            uint sourceId = info.SourceId;
            Angle rotation = info.Facing + 90.Degrees();
            SimpleElement.Rectangle(sourceId, 42f, 22f, 0f, null, rotation, 3000f, 0f, 24537u);
        }
    }
}
