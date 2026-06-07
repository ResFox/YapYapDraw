using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class AscalonsMercyRevealedP2 : ISpecialAction
{
    public override string Name => "Ascalon's Mercy Revealed";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25545u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                radiusX = 50f,
                radiusZ = 50f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            }, source);
        }
    }
}
