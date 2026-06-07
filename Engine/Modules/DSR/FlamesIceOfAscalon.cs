using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class FlamesIceOfAscalon : ISpecialAction
{
    public override string Name => "Ice / Flames of Ascalon";

    public override uint Phase => 7u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 2056)
        {
            return;
        }
        IGameObject target = info.TargetID.GameObject();
        if (target.BaseId == 12616)
        {
            if (info.Stack == 298)
            {
                SimpleElement.Circle(target, 8f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 28049u }
                });
            }
            else
            {
                SimpleElement.Donut(target, 8f, 50f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 28050u }
                });
            }
        }
    }
}
