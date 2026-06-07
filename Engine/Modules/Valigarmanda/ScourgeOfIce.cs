using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Valigarmanda;

public class ScourgeOfIce : ISpecialAction
{
    public override string Name => "Scourge of Ice";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 509)
        {
            uint weatherID = Events.WeatherID;
            if (weatherID == 15)
            {
                SimpleElement.Circle(target, 16f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 36844u }
                });
            }
            if (weatherID == 14 && target.GameObjectId == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                SimpleElement.ShowText("Move now", (TextGimmickHintStyle)1, 7);
            }
            if (weatherID == 9 && target.GameObjectId == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                SimpleElement.ShowText("Float up", (TextGimmickHintStyle)1, 7);
            }
        }
    }
}
