using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class FutureSEndSpread : ISpecialAction
{
    public override string Name => "Future's End β (spread)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18592u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "loc06sp_05ak1",
                delayDrawTime = 28000f
            }, allPlayer);
        }
    }
}
