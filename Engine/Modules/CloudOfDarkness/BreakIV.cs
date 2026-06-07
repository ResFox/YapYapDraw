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

namespace YapYapDraw.Modules.CloudOfDarkness;

public class BreakIV : ISpecialAction
{
    public override string Name => "Break IV (look away)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40527u, 40530u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawType = ElementType.Channeling,
            drawAvfx = "chn_chainlightning_3t1",
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40530u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer, info.SourceId.GameObject());
        DrawManager.Draw(new DrawElement
        {
            drawType = ElementType.Channeling,
            drawAvfx = "chn_miruna1v",
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40530u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer, info.SourceId.GameObject());
    }
}
