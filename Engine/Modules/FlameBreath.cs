using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M11S;

public class FlameBreath : ISpecialAction
{
    public override string Name => "Flame Breath";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46144u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 46144)
        {
            base.CanDraw = true;
        }
    }

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        if (icon != 244 || !base.CanDraw)
        {
            return;
        }
        IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
        if (TargetID != ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
        {
            SimpleElement.RectangleToTarget(Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19180), Source, 60f, 3f, 3000f, new HitCounter
            {
                ActionID = new HashSet<uint> { 46151u }
            });
        }
    }
}
