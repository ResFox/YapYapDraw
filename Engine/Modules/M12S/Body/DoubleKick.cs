using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M12S.Body;

public class DoubleKick : ISpecialAction
{
    public override string Name => "Double Kick";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 46368u, 46373u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 46368)
        {
            SimpleElement.Fan(info.SourceId, 60f, 180, info.SourceId.GameObject().Rotation.Radians(), 3000f, 0f, 0u);
            return;
        }
        SimpleElement.Fan(info, 180);
        DrawElement element = new DrawElement
        {
            drawAvfx = "tank_lockonae_10m_7s_01w",
            drawType = ElementType.LockOn,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 46374u }
            }
        };
        foreach (IGameObject tank in PlayerHelper.Tank)
        {
            DrawManager.Draw(element, tank);
        }
    }
}
