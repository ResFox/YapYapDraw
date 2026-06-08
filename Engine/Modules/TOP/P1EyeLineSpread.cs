using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class P1EyeLineSpread : ISpecialAction
{
    public override string Name => "P1 Eye Line + Spread";

    public override uint Phase => 2u;

    public override uint WeatherID => 78u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31550u, 31525u, 31526u, 31531u, 31532u, 31533u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 31550)
        {
            base.CanDraw = true;
        }
        uint actionId = info.ActionId;
        bool flag = actionId - 31525 <= 1 || actionId - 31531 <= 2;
        if (!flag || !base.CanDraw)
        {
            return;
        }
        base.CanDraw = false;
        IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15716);
        Angle rotation = target.Rotation.Radians();
        SimpleElement.Rectangle(target, 100f, 8f, 0f, null, rotation, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 31521u }
        });
        ulong localId = ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId;
        foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1 && o.GameObjectId != localId))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 7f,
                radiusZ = 7f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31535u }
                }
            }, item);
        }
    }
}
