using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.GolbezEx;
public class AethericShockwaveAethericCannon : ISpecialAction
{
    public override string Name => "Aetheric Shockwave / Aetheric Cannon";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        switch (icon)
        {
        case 637u:
        {
            foreach (IGameObject healer in PlayerHelper.Healer)
            {
                SimpleLockon.ShareLockon2(healer, 1500f);
            }
            break;
        }
        case 638u:
        {
            foreach (IGameObject player in PlayerHelper.DPS.Union(PlayerHelper.Healer))
            {
                SimpleLockon.TarLockOn6m5s(player, 2500f);
            }
            break;
        }
        }
    }
}
