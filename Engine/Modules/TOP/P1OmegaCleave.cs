using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class P1OmegaCleave : ISpecialAction
{
    public override string Name => "P1 Omega (cleave)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31636u, 31637u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 210);
        DrawElement element = new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 4f,
            radiusZ = 4f,
            drawOnObject = true,
            delayDrawTime = (int)((info.CastTime - 3f) * 1000f),
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        };
        foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1 && o.GameObjectId != ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId))
        {
            switch (item.GameObjectId.Mark())
            {
            case HeaderMarkerEnum.Attack1:
            case HeaderMarkerEnum.Attack2:
            case HeaderMarkerEnum.Attack3:
            case HeaderMarkerEnum.Attack4:
                DrawManager.Draw(element, item);
                break;
            }
        }
    }
}
