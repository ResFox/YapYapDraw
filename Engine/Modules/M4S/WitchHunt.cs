using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class WitchHunt : ISpecialAction
{
    public override string Name => "Witch Hunt (bait)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38366u };

    public override void OnActionCast(ActorCastInfo info)
    {
        int param;
        bool moveIn = StatusHelper.GetParam(info.SourceId, 2970u, out param) && param == 758;
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                drawOnObject = true,
                radiusX = 6f,
                radiusZ = 6f,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = (moveIn ? 2 : 3),
                    Count = 4
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38375u }
                }
            }, allPlayer);
        }
        if (!((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(587u))
        {
            SimpleElement.ShowText((moveIn ? "Move in" : "Move out") + "(bait)", (TextGimmickHintStyle)0);
        }
    }
}
