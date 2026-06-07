using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56456;

public class WickedMist : ISpecialAction
{
    public override string Name => "Wicked Mist";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36529u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customDonut",
            Position = new Vector3(0f, -29f, 150f),
            drawOnObject = false,
            refRadian = 0.7f,
            radiusX = 20f,
            radiusZ = 20f,
            destroyTime = 5000f,
            refColor = GroundOmen.enemyColor,
            refTargetColor = GroundOmen.enemyColor
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
