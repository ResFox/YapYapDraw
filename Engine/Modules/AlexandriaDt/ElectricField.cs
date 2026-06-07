using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.AlexandriaDt;

public class ElectricField : ISpecialAction
{
    public override string Name => "Electric Field";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43261u };

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 586)
        {
            IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
            if (TargetID != ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
            {
                IGameObject? target2 = TargetID.GameObject();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = ActionID
                };
                SimpleElement.FanToTarget(target, target2, 26f, 50, Follow: true, default, 0f, 3000f, hitCounter);
            }
        }
    }
}
