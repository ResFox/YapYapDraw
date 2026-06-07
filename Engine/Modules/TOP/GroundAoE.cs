using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class GroundAoE : ISpecialAction
{
    public override string Name => "Ground AoE";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31567u, 31568u, 31569u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        switch (info.ActionId)
        {
        case 31567u:
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customDonut",
                Position = new Vector3(100f, 0f, 100f),
                drawOnObject = false,
                refRadian = 0.5f,
                radiusX = 12f,
                radiusZ = 12f,
                refColor = GroundOmen.enemyColor,
                refTargetColor = GroundOmen.enemyColor,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31568u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            break;
        case 31568u:
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customDonut",
                Position = new Vector3(100f, 0f, 100f),
                drawOnObject = false,
                refRadian = 2f / 3f,
                radiusX = 18f,
                radiusZ = 18f,
                refColor = GroundOmen.enemyColor,
                refTargetColor = GroundOmen.enemyColor,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31569u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            break;
        case 31569u:
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customDonut",
                Position = new Vector3(100f, 0f, 100f),
                drawOnObject = false,
                refRadian = 0.75f,
                radiusX = 24f,
                radiusZ = 24f,
                refColor = GroundOmen.enemyColor,
                refTargetColor = GroundOmen.enemyColor,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31570u }
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            break;
        }
    }
}
