using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class WickedTornado : ISpecialAction
{
    public override string Name => "P1 Wicked Tornado";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11126u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 11126)
        {
            base.CanDraw = true;
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (base.CanDraw && source.BaseId == 8722 && id == 7747)
        {
            base.CanDraw = false;
            SimpleElement.Circle(source, 20f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 11087u }
            });
        }
    }
}
