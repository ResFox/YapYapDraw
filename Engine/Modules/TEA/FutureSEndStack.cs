using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class FutureSEndStack : ISpecialAction
{
    public override string Name => "Future's End β (stack)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18593u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawType = ElementType.LockOn,
            drawAvfx = "com_share0c",
            delayDrawTime = 28000f
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
