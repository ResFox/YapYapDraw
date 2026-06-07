using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class Heavensblaze : ISpecialAction
{
    public override string Name => "Heavensblaze";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25309u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject target = Svc.Objects.SearchById((ulong)info.TargetId);
        if (target != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "com_share0c",
                drawOnObject = true
            }, target);
        }
    }
}
