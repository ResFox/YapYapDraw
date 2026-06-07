using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P2;

public class DestructionWing : ISpecialAction
{
    public override string Name => "Destruction Wing";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47821u, 47822u, 50311u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId is 47821 or 47822)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02wf",
                Position = info.Pos,
                drawOnObject = false,
                radiusX = 20f,
                radiusZ = 80f,
                refRotation = info.Facing,
                hitCounter = new HitCounter
                {
                    ActionID = ActionID
                }
            });
        }

        if (info.ActionId != 50311)
            return;

        foreach (IGameObject player in PlayerHelper.AllPlayers)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "tank_lockon_5m_5s_noc",
                refColor = GroundOmen.Red,
                refTargetColor = GroundOmen.Red,
                radiusX = 7f,
                radiusZ = 7f,
                distanceCheck = new DistanceCheck
                {
                    CheckType = 2,
                    CheckObject = info.SourceId.GameObject()
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47823u }
                }
            };
            DrawManager.Draw(element, player);

            element.distanceCheck = new DistanceCheck
            {
                CheckType = 3,
                CheckObject = info.SourceId.GameObject()
            };
            DrawManager.Draw(element, player);
        }
    }
}
