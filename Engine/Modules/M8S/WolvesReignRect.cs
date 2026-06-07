using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M8S;

public class WolvesReignRect : ISpecialAction
{
    public override string Name => "Wolves' Reign(Rect)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43312u, 43313u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        SimpleElement.Rectangle((new WPos(info.Source.Position) - 4f * info.Rotation.ToDirection()).ToVec3(), 28f, 5f, 0f, info.Rotation, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 43369u, 43370u }
        });
    }
}
