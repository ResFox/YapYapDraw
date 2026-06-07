using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class LightningStorm : ISpecialAction
{
    public override string Name => "Lightning Storm";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25548u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0420tar_5m0f",
                drawType = ElementType.LockOn,
                drawOnObject = true
            }, item);
        }
    }
}
