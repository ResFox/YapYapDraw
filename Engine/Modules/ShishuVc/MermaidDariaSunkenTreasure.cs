using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ShishuVc;

public class MermaidDariaSunkenTreasure : ISpecialAction
{
    public override string Name => "Mermaid Daria Sunken Treasure";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45849u };

    public override void OnActionCast(ActorCastInfo info)
    {
        Reset();
    }

    public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
    {
        IGameObject eventObj = actorID.GameObject();
        switch (eventObj.BaseId)
        {
        case 2015004u:
            if (p1 == 16 && p2 == 32)
            {
                SimpleElement.Circle(eventObj.Position, 18f, (base.NumCasts > 0) ? 3500 : 10000, (base.NumCasts > 0) ? 6500 : 0);
                base.NumCasts++;
            }
            break;
        case 2015005u:
            if (p1 == 16 && p2 == 32)
            {
                SimpleElement.Donut(eventObj.Position, 5f, 20f, 10000f);
            }
            break;
        }
    }
}
