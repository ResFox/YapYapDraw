using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M7S;

public class QuarrySwamp : ISpecialAction
{
    public override string Name => "Quarry Swamp";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42357u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawType = ElementType.Channeling,
            drawAvfx = "chn_miruna1v",
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 42357u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer, info.SourceId.GameObject());
    }
}
