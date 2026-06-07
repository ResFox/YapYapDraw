using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M3S;

public class BombarianSpecial : ISpecialAction
{
    public override string Name => "Bombarian Special";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38738u, 37898u, 37904u, 37905u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 38738:
            SimpleElement.ShowText("Spread soon", (TextGimmickHintStyle)0);
            {
                foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
                {
                    SimpleLockon.TarLockOn6m5s(allPlayer, 25700f);
                }
                break;
            }
        case 37898:
            SimpleElement.ShowText("2+2 stacks soon", (TextGimmickHintStyle)0);
            foreach (IGameObject tank in PlayerHelper.Tank)
            {
                SimpleLockon.ShareLockon2(tank, 25700f);
            }
            {
                foreach (IGameObject healer in PlayerHelper.Healer)
                {
                    SimpleLockon.ShareLockon2(healer, 25700f);
                }
                break;
            }
        case 37904:
            SimpleElement.Circle(info, 10f, 12700f);
            break;
        case 37905:
            SimpleElement.Donut(info, 6f, 40f, 15800f);
            break;
        }
    }
}
