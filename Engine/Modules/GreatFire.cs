using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M11S;

public class GreatFire : ISpecialAction
{
    public override string Name => "Great Fire";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46138u };

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        if (icon == 525)
        {
            SimpleElement.RectangleToTarget(Source, TargetID.GameObject(), 60f, 3f, 3000f, new HitCounter
            {
                ActionID = new HashSet<uint> { 46138u }
            });
        }
    }
}
