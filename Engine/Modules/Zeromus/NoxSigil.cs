using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Zeromus;

public class NoxSigil : ISpecialAction
{
    public override string Name => "Nox Sigil";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 35685u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject target = Svc.Objects.SearchById((ulong)info.TargetId);
        if (target != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "m0618trg_a0k1",
                delayDrawTime = 5000f
            }, target);
        }
    }
}
